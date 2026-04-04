using coderush.Data;
using coderush.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace coderush.Services
{
    public class ChatBotService(
        HttpClient httpClient,
        IOptions<OpenAIOptions> options,
        ApplicationDbContext context,
        INumberSequence numberSequence) : IChatBotService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly OpenAIOptions _options = options.Value;
        private readonly ApplicationDbContext _context = context;
        private readonly INumberSequence _numberSequence = numberSequence;

        private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = false };

        private const string BaseSystemPrompt =
            "You are an AI assistant for an Inventory Order Management System. " +
            "You help users manage their:\n" +
            "- Products (inventory items with buying/selling prices)\n" +
            "- Customers and Customer Types\n" +
            "- Vendors and Vendor Types\n" +
            "- Sales Orders (linked to customers)\n" +
            "- Purchase Orders (linked to vendors)\n" +
            "- Shipments (fulfillment of sales orders)\n" +
            "- Invoices (billing for shipments)\n" +
            "- Goods Received Notes (receiving purchase orders)\n" +
            "- Bills (vendor invoices for goods received)\n" +
            "- Payment Receives (customer payments for invoices)\n" +
            "- Payment Vouchers (vendor payments for bills)\n" +
            "- Warehouses, Branches, Currencies, Cash Banks\n\n" +
            "You can query data, create new records, edit existing records, and delete existing records using the available tools. " +
            "When users ask about inventory or want to perform operations, use the tools.\n\n" +
            "Users can also upload files (.txt, .md, .docx, .xlsx, .xls) for you to analyze. " +
            "When a file is uploaded, its content will be provided. You should:\n" +
            "- Summarize the file contents if asked\n" +
            "- Extract data (e.g. product lists, customer info, vendor details, order data) from the file\n" +
            "- Help import/create records from file data using the available tools when the user requests it\n" +
            "- Answer questions about the file contents\n\n" +
            "Be concise and helpful. Always confirm actions taken with details of what was created or modified. " +
            "If you need more information to complete a request, ask the user.\n" +
            "When listing data, format it in a clean readable way using markdown tables or bullet points.";

        // Maps each tool function name to the role required to use it.
        // null means no specific role is required (available to any authenticated user).
        private static readonly Dictionary<string, string> FunctionRoleMap = new()
        {
            ["get_inventory_summary"] = null,
            ["list_products"] = Pages.MainMenu.Product.RoleName,
            ["search_products"] = Pages.MainMenu.Product.RoleName,
            ["create_product"] = Pages.MainMenu.Product.RoleName,
            ["delete_product"] = Pages.MainMenu.Product.RoleName,
            ["list_customers"] = Pages.MainMenu.Customer.RoleName,
            ["search_customers"] = Pages.MainMenu.Customer.RoleName,
            ["create_customer"] = Pages.MainMenu.Customer.RoleName,
            ["delete_customer"] = Pages.MainMenu.Customer.RoleName,
            ["list_vendors"] = Pages.MainMenu.Vendor.RoleName,
            ["search_vendors"] = Pages.MainMenu.Vendor.RoleName,
            ["create_vendor"] = Pages.MainMenu.Vendor.RoleName,
            ["delete_vendor"] = Pages.MainMenu.Vendor.RoleName,
            ["list_sales_orders"] = Pages.MainMenu.SalesOrder.RoleName,
            ["search_sales_orders"] = Pages.MainMenu.SalesOrder.RoleName,
            ["create_sales_order"] = Pages.MainMenu.SalesOrder.RoleName,
            ["delete_sales_order"] = Pages.MainMenu.SalesOrder.RoleName,
            ["list_purchase_orders"] = Pages.MainMenu.PurchaseOrder.RoleName,
            ["search_purchase_orders"] = Pages.MainMenu.PurchaseOrder.RoleName,
            ["create_purchase_order"] = Pages.MainMenu.PurchaseOrder.RoleName,
            ["delete_purchase_order"] = Pages.MainMenu.PurchaseOrder.RoleName,
            ["list_warehouses"] = Pages.MainMenu.Warehouse.RoleName,
            ["search_warehouses"] = Pages.MainMenu.Warehouse.RoleName,
            ["create_warehouse"] = Pages.MainMenu.Warehouse.RoleName,
            ["delete_warehouse"] = Pages.MainMenu.Warehouse.RoleName,
            ["list_branches"] = Pages.MainMenu.Branch.RoleName,
            ["search_branches"] = Pages.MainMenu.Branch.RoleName,
            ["create_branch"] = Pages.MainMenu.Branch.RoleName,
            ["delete_branch"] = Pages.MainMenu.Branch.RoleName,
            ["list_shipments"] = Pages.MainMenu.Shipment.RoleName,
            ["search_shipments"] = Pages.MainMenu.Shipment.RoleName,
            ["create_shipment"] = Pages.MainMenu.Shipment.RoleName,
            ["delete_shipment"] = Pages.MainMenu.Shipment.RoleName,
            ["list_invoices"] = Pages.MainMenu.Invoice.RoleName,
            ["search_invoices"] = Pages.MainMenu.Invoice.RoleName,
            ["create_invoice"] = Pages.MainMenu.Invoice.RoleName,
            ["delete_invoice"] = Pages.MainMenu.Invoice.RoleName,
            ["list_bills"] = Pages.MainMenu.Bill.RoleName,
            ["search_bills"] = Pages.MainMenu.Bill.RoleName,
            ["create_bill"] = Pages.MainMenu.Bill.RoleName,
            ["delete_bill"] = Pages.MainMenu.Bill.RoleName,
            ["list_goods_received_notes"] = Pages.MainMenu.GoodsReceivedNote.RoleName,
            ["search_goods_received_notes"] = Pages.MainMenu.GoodsReceivedNote.RoleName,
            ["create_goods_received_note"] = Pages.MainMenu.GoodsReceivedNote.RoleName,
            ["delete_goods_received_note"] = Pages.MainMenu.GoodsReceivedNote.RoleName,
            ["list_payment_vouchers"] = Pages.MainMenu.PaymentVoucher.RoleName,
            ["search_payment_vouchers"] = Pages.MainMenu.PaymentVoucher.RoleName,
            ["create_payment_voucher"] = Pages.MainMenu.PaymentVoucher.RoleName,
            ["delete_payment_voucher"] = Pages.MainMenu.PaymentVoucher.RoleName,
            ["list_payment_receives"] = Pages.MainMenu.PaymentReceive.RoleName,
            ["search_payment_receives"] = Pages.MainMenu.PaymentReceive.RoleName,
            ["create_payment_receive"] = Pages.MainMenu.PaymentReceive.RoleName,
            ["delete_payment_receive"] = Pages.MainMenu.PaymentReceive.RoleName,
            ["list_cash_banks"] = Pages.MainMenu.CashBank.RoleName,
            ["search_cash_banks"] = Pages.MainMenu.CashBank.RoleName,
            ["create_cash_bank"] = Pages.MainMenu.CashBank.RoleName,
            ["delete_cash_bank"] = Pages.MainMenu.CashBank.RoleName,
            ["list_currencies"] = Pages.MainMenu.Currency.RoleName,
            ["search_currencies"] = Pages.MainMenu.Currency.RoleName,
            ["create_currency"] = Pages.MainMenu.Currency.RoleName,
            ["delete_currency"] = Pages.MainMenu.Currency.RoleName,
            ["list_bill_types"] = Pages.MainMenu.BillType.RoleName,
            ["search_bill_types"] = Pages.MainMenu.BillType.RoleName,
            ["create_bill_type"] = Pages.MainMenu.BillType.RoleName,
            ["delete_bill_type"] = Pages.MainMenu.BillType.RoleName,
            ["list_customer_types"] = Pages.MainMenu.CustomerType.RoleName,
            ["search_customer_types"] = Pages.MainMenu.CustomerType.RoleName,
            ["create_customer_type"] = Pages.MainMenu.CustomerType.RoleName,
            ["delete_customer_type"] = Pages.MainMenu.CustomerType.RoleName,
            ["list_invoice_types"] = Pages.MainMenu.InvoiceType.RoleName,
            ["search_invoice_types"] = Pages.MainMenu.InvoiceType.RoleName,
            ["create_invoice_type"] = Pages.MainMenu.InvoiceType.RoleName,
            ["delete_invoice_type"] = Pages.MainMenu.InvoiceType.RoleName,
            ["list_payment_types"] = Pages.MainMenu.PaymentType.RoleName,
            ["search_payment_types"] = Pages.MainMenu.PaymentType.RoleName,
            ["create_payment_type"] = Pages.MainMenu.PaymentType.RoleName,
            ["delete_payment_type"] = Pages.MainMenu.PaymentType.RoleName,
            ["list_product_types"] = Pages.MainMenu.ProductType.RoleName,
            ["search_product_types"] = Pages.MainMenu.ProductType.RoleName,
            ["create_product_type"] = Pages.MainMenu.ProductType.RoleName,
            ["delete_product_type"] = Pages.MainMenu.ProductType.RoleName,
            ["list_sales_types"] = Pages.MainMenu.SalesType.RoleName,
            ["search_sales_types"] = Pages.MainMenu.SalesType.RoleName,
            ["create_sales_type"] = Pages.MainMenu.SalesType.RoleName,
            ["delete_sales_type"] = Pages.MainMenu.SalesType.RoleName,
            ["list_shipment_types"] = Pages.MainMenu.ShipmentType.RoleName,
            ["search_shipment_types"] = Pages.MainMenu.ShipmentType.RoleName,
            ["create_shipment_type"] = Pages.MainMenu.ShipmentType.RoleName,
            ["delete_shipment_type"] = Pages.MainMenu.ShipmentType.RoleName,
            ["list_units_of_measure"] = Pages.MainMenu.UnitOfMeasure.RoleName,
            ["search_units_of_measure"] = Pages.MainMenu.UnitOfMeasure.RoleName,
            ["create_unit_of_measure"] = Pages.MainMenu.UnitOfMeasure.RoleName,
            ["delete_unit_of_measure"] = Pages.MainMenu.UnitOfMeasure.RoleName,
            ["list_vendor_types"] = Pages.MainMenu.VendorType.RoleName,
            ["search_vendor_types"] = Pages.MainMenu.VendorType.RoleName,
            ["create_vendor_type"] = Pages.MainMenu.VendorType.RoleName,
            ["delete_vendor_type"] = Pages.MainMenu.VendorType.RoleName,
            ["list_purchase_types"] = Pages.MainMenu.PurchaseType.RoleName,
            ["search_purchase_types"] = Pages.MainMenu.PurchaseType.RoleName,
            ["create_purchase_type"] = Pages.MainMenu.PurchaseType.RoleName,
            ["delete_purchase_type"] = Pages.MainMenu.PurchaseType.RoleName,
            ["edit_product"] = Pages.MainMenu.Product.RoleName,
            ["edit_customer"] = Pages.MainMenu.Customer.RoleName,
            ["edit_vendor"] = Pages.MainMenu.Vendor.RoleName,
            ["edit_sales_order"] = Pages.MainMenu.SalesOrder.RoleName,
            ["edit_purchase_order"] = Pages.MainMenu.PurchaseOrder.RoleName,
            ["edit_warehouse"] = Pages.MainMenu.Warehouse.RoleName,
            ["edit_branch"] = Pages.MainMenu.Branch.RoleName,
            ["edit_shipment"] = Pages.MainMenu.Shipment.RoleName,
            ["edit_invoice"] = Pages.MainMenu.Invoice.RoleName,
            ["edit_bill"] = Pages.MainMenu.Bill.RoleName,
            ["edit_goods_received_note"] = Pages.MainMenu.GoodsReceivedNote.RoleName,
            ["edit_payment_voucher"] = Pages.MainMenu.PaymentVoucher.RoleName,
            ["edit_payment_receive"] = Pages.MainMenu.PaymentReceive.RoleName,
            ["edit_cash_bank"] = Pages.MainMenu.CashBank.RoleName,
            ["edit_currency"] = Pages.MainMenu.Currency.RoleName,
            ["edit_bill_type"] = Pages.MainMenu.BillType.RoleName,
            ["edit_customer_type"] = Pages.MainMenu.CustomerType.RoleName,
            ["edit_invoice_type"] = Pages.MainMenu.InvoiceType.RoleName,
            ["edit_payment_type"] = Pages.MainMenu.PaymentType.RoleName,
            ["edit_product_type"] = Pages.MainMenu.ProductType.RoleName,
            ["edit_sales_type"] = Pages.MainMenu.SalesType.RoleName,
            ["edit_shipment_type"] = Pages.MainMenu.ShipmentType.RoleName,
            ["edit_unit_of_measure"] = Pages.MainMenu.UnitOfMeasure.RoleName,
            ["edit_vendor_type"] = Pages.MainMenu.VendorType.RoleName,
            ["edit_purchase_type"] = Pages.MainMenu.PurchaseType.RoleName,
            ["get_dashboard_data"] = Pages.MainMenu.Dashboard.RoleName,
        };

        private static readonly List<object> AllToolDefinitions = BuildTools();

        public async Task<string> ChatAsync(string userMessage, List<ChatMessageDto> history, IList<string> userRoles, List<ChatFileContent> files = null, CancellationToken cancellationToken = default)
        {
            IList<string> roles = userRoles ?? Array.Empty<string>();

            // Build role-aware system prompt
            var systemPrompt = BuildSystemPrompt(roles);

            // Filter tools to only those the user has permission to use
            List<object> allowedTools = GetAllowedTools(roles);

            var messages = new List<object>
            {
                new Dictionary<string, object> { ["role"] = "system", ["content"] = systemPrompt }
            };

            if (history != null)
            {
                foreach (ChatMessageDto msg in history)
                {
                    messages.Add(new Dictionary<string, object> { ["role"] = msg.Role, ["content"] = msg.Content });
                }
            }

            // If files are provided, prepend their content as context to the user message
            var effectiveMessage = userMessage;
            if (files != null && files.Count > 0)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"The user uploaded {files.Count} file(s):");
                for (int i = 0; i < files.Count; i++)
                {
                    sb.AppendLine($"\n--- FILE {i + 1}: \"{files[i].FileName}\" ---");
                    sb.AppendLine(files[i].Content);
                    sb.AppendLine($"--- END FILE {i + 1} ---");
                }
                sb.AppendLine();
                sb.Append($"User message: {userMessage}");
                effectiveMessage = sb.ToString();
            }

            messages.Add(new Dictionary<string, object> { ["role"] = "user", ["content"] = effectiveMessage });

            for (int iteration = 0; iteration < 5; iteration++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var requestBody = new Dictionary<string, object>
                {
                    ["model"] = _options.Model ?? "gpt-4o-mini",
                    ["messages"] = messages,
                    ["tools"] = allowedTools,
                    ["temperature"] = 0.7
                };

                var json = JsonSerializer.Serialize(requestBody);
                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions")
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
                request.Headers.Add("Authorization", $"Bearer {_options.ApiKey}");

                HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
                var responseStr = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    return "I'm sorry, I encountered an error communicating with the AI service. " +
                           "Please ensure the OpenAI API key is configured correctly in appsettings.json.";
                }

                using var doc = JsonDocument.Parse(responseStr);
                JsonElement choice = doc.RootElement.GetProperty("choices")[0];
                JsonElement message = choice.GetProperty("message");

                if (message.TryGetProperty("tool_calls", out JsonElement toolCalls) && toolCalls.GetArrayLength() > 0)
                {
                    // Build assistant message with tool_calls for the conversation
                    var toolCallsList = toolCalls.EnumerateArray().Select(tc => new Dictionary<string, object>
                    {
                        ["id"] = tc.GetProperty("id").GetString(),
                        ["type"] = "function",
                        ["function"] = new Dictionary<string, object>
                        {
                            ["name"] = tc.GetProperty("function").GetProperty("name").GetString(),
                            ["arguments"] = tc.GetProperty("function").GetProperty("arguments").GetString()
                        }
                    }).ToList<object>();
                    
                    var hasContent = message.TryGetProperty("content", out JsonElement contentEl) && contentEl.ValueKind != JsonValueKind.Null;
                    Dictionary<string, object> assistantMsg = hasContent 
                        ? new Dictionary<string, object> 
                        { 
                            ["role"] = "assistant",
                            ["tool_calls"] = toolCallsList,
                            ["content"] = contentEl.GetString()
                        }
                        : new Dictionary<string, object> 
                        { 
                            ["role"] = "assistant",
                            ["tool_calls"] = toolCallsList
                        };
                    
                    messages.Add(assistantMsg);

                    // Execute each tool call and add results
                    foreach (JsonElement tc in toolCalls.EnumerateArray())
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var toolCallId = tc.GetProperty("id").GetString();
                        var functionName = tc.GetProperty("function").GetProperty("name").GetString();
                        var arguments = tc.GetProperty("function").GetProperty("arguments").GetString();

                        var result = await ExecuteFunctionAsync(functionName, arguments, roles);
                        messages.Add(new Dictionary<string, object>
                        {
                            ["role"] = "tool",
                            ["tool_call_id"] = toolCallId,
                            ["content"] = result
                        });
                    }
                }
                else
                {
                    if (message.TryGetProperty("content", out JsonElement contentEl) && contentEl.ValueKind != JsonValueKind.Null)
                        return contentEl.GetString();

                    return "I'm sorry, I couldn't generate a response. Please try again.";
                }
            }

            return "I reached the maximum number of processing steps. Please try a simpler request.";
        }

        private async Task<string> ExecuteFunctionAsync(string functionName, string argumentsJson, IList<string> userRoles)
        {
            try
            {
                // Server-side role enforcement: reject the call if user lacks the required role
                if (FunctionRoleMap.TryGetValue(functionName, out var requiredRole)
                    && requiredRole != null
                    && !userRoles.Contains(requiredRole))
                {
                    return Serialize(new { Success = false, Message = $"Access denied. You do not have the '{requiredRole}' role required to perform this action." });
                }

                JsonElement args = JsonDocument.Parse(argumentsJson).RootElement;

                return functionName switch
                {
                    "get_inventory_summary" => await GetInventorySummary(),
                    "list_products" => await ListProducts(),
                    "search_products" => await SearchProducts(GetStr(args, "query")),
                    "create_product" => await CreateProduct(args),
                    "delete_product" => await DeleteProduct(GetInt(args, "productId")),
                    "list_customers" => await ListCustomers(),
                    "search_customers" => await SearchCustomers(GetStr(args, "query")),
                    "create_customer" => await CreateCustomer(args),
                    "delete_customer" => await DeleteCustomer(GetInt(args, "customerId")),
                    "list_vendors" => await ListVendors(),
                    "search_vendors" => await SearchVendors(GetStr(args, "query")),
                    "create_vendor" => await CreateVendor(args),
                    "delete_vendor" => await DeleteVendor(GetInt(args, "vendorId")),
                    "list_sales_orders" => await ListSalesOrders(),
                    "search_sales_orders" => await SearchSalesOrders(GetStr(args, "query")),
                    "create_sales_order" => await CreateSalesOrder(args),
                    "delete_sales_order" => await DeleteSalesOrder(GetInt(args, "salesOrderId")),
                    "list_purchase_orders" => await ListPurchaseOrders(),
                    "search_purchase_orders" => await SearchPurchaseOrders(GetStr(args, "query")),
                    "create_purchase_order" => await CreatePurchaseOrder(args),
                    "delete_purchase_order" => await DeletePurchaseOrder(GetInt(args, "purchaseOrderId")),
                    "list_warehouses" => await ListWarehouses(),
                    "search_warehouses" => await SearchWarehouses(GetStr(args, "query")),
                    "create_warehouse" => await CreateWarehouse(args),
                    "delete_warehouse" => await DeleteWarehouse(GetInt(args, "warehouseId")),
                    "list_branches" => await ListBranches(),
                    "search_branches" => await SearchBranches(GetStr(args, "query")),
                    "create_branch" => await CreateBranch(args),
                    "delete_branch" => await DeleteBranch(GetInt(args, "branchId")),
                    "list_shipments" => await ListShipments(),
                    "search_shipments" => await SearchShipments(GetStr(args, "query")),
                    "create_shipment" => await CreateShipment(args),
                    "delete_shipment" => await DeleteShipment(GetInt(args, "shipmentId")),
                    "list_invoices" => await ListInvoices(),
                    "search_invoices" => await SearchInvoices(GetStr(args, "query")),
                    "create_invoice" => await CreateInvoice(args),
                    "delete_invoice" => await DeleteInvoice(GetInt(args, "invoiceId")),
                    "list_bills" => await ListBills(),
                    "search_bills" => await SearchBills(GetStr(args, "query")),
                    "create_bill" => await CreateBill(args),
                    "delete_bill" => await DeleteBill(GetInt(args, "billId")),
                    "list_goods_received_notes" => await ListGoodsReceivedNotes(),
                    "search_goods_received_notes" => await SearchGoodsReceivedNotes(GetStr(args, "query")),
                    "create_goods_received_note" => await CreateGoodsReceivedNote(args),
                    "delete_goods_received_note" => await DeleteGoodsReceivedNote(GetInt(args, "goodsReceivedNoteId")),
                    "list_payment_vouchers" => await ListPaymentVouchers(),
                    "search_payment_vouchers" => await SearchPaymentVouchers(GetStr(args, "query")),
                    "create_payment_voucher" => await CreatePaymentVoucher(args),
                    "delete_payment_voucher" => await DeletePaymentVoucher(GetInt(args, "paymentVoucherId")),
                    "list_payment_receives" => await ListPaymentReceives(),
                    "search_payment_receives" => await SearchPaymentReceives(GetStr(args, "query")),
                    "create_payment_receive" => await CreatePaymentReceive(args),
                    "delete_payment_receive" => await DeletePaymentReceive(GetInt(args, "paymentReceiveId")),
                    "list_cash_banks" => await ListCashBanks(),
                    "search_cash_banks" => await SearchCashBanks(GetStr(args, "query")),
                    "create_cash_bank" => await CreateCashBank(args),
                    "delete_cash_bank" => await DeleteCashBank(GetInt(args, "cashBankId")),
                    "list_currencies" => await ListCurrencies(),
                    "search_currencies" => await SearchCurrencies(GetStr(args, "query")),
                    "create_currency" => await CreateCurrency(args),
                    "delete_currency" => await DeleteCurrency(GetInt(args, "currencyId")),
                    "list_bill_types" => await ListBillTypes(),
                    "search_bill_types" => await SearchBillTypes(GetStr(args, "query")),
                    "create_bill_type" => await CreateBillType(args),
                    "delete_bill_type" => await DeleteBillType(GetInt(args, "billTypeId")),
                    "list_customer_types" => await ListCustomerTypes(),
                    "search_customer_types" => await SearchCustomerTypes(GetStr(args, "query")),
                    "create_customer_type" => await CreateCustomerType(args),
                    "delete_customer_type" => await DeleteCustomerType(GetInt(args, "customerTypeId")),
                    "list_invoice_types" => await ListInvoiceTypes(),
                    "search_invoice_types" => await SearchInvoiceTypes(GetStr(args, "query")),
                    "create_invoice_type" => await CreateInvoiceType(args),
                    "delete_invoice_type" => await DeleteInvoiceType(GetInt(args, "invoiceTypeId")),
                    "list_payment_types" => await ListPaymentTypes(),
                    "search_payment_types" => await SearchPaymentTypes(GetStr(args, "query")),
                    "create_payment_type" => await CreatePaymentType(args),
                    "delete_payment_type" => await DeletePaymentType(GetInt(args, "paymentTypeId")),
                    "list_product_types" => await ListProductTypes(),
                    "search_product_types" => await SearchProductTypes(GetStr(args, "query")),
                    "create_product_type" => await CreateProductType(args),
                    "delete_product_type" => await DeleteProductType(GetInt(args, "productTypeId")),
                    "list_sales_types" => await ListSalesTypes(),
                    "search_sales_types" => await SearchSalesTypes(GetStr(args, "query")),
                    "create_sales_type" => await CreateSalesType(args),
                    "delete_sales_type" => await DeleteSalesType(GetInt(args, "salesTypeId")),
                    "list_shipment_types" => await ListShipmentTypes(),
                    "search_shipment_types" => await SearchShipmentTypes(GetStr(args, "query")),
                    "create_shipment_type" => await CreateShipmentType(args),
                    "delete_shipment_type" => await DeleteShipmentType(GetInt(args, "shipmentTypeId")),
                    "list_units_of_measure" => await ListUnitsOfMeasure(),
                    "search_units_of_measure" => await SearchUnitsOfMeasure(GetStr(args, "query")),
                    "create_unit_of_measure" => await CreateUnitOfMeasure(args),
                    "delete_unit_of_measure" => await DeleteUnitOfMeasure(GetInt(args, "unitOfMeasureId")),
                    "list_vendor_types" => await ListVendorTypes(),
                    "search_vendor_types" => await SearchVendorTypes(GetStr(args, "query")),
                    "create_vendor_type" => await CreateVendorType(args),
                    "delete_vendor_type" => await DeleteVendorType(GetInt(args, "vendorTypeId")),
                    "list_purchase_types" => await ListPurchaseTypes(),
                    "search_purchase_types" => await SearchPurchaseTypes(GetStr(args, "query")),
                    "create_purchase_type" => await CreatePurchaseType(args),
                    "delete_purchase_type" => await DeletePurchaseType(GetInt(args, "purchaseTypeId")),
                    "edit_product" => await EditProduct(args),
                    "edit_customer" => await EditCustomer(args),
                    "edit_vendor" => await EditVendor(args),
                    "edit_sales_order" => await EditSalesOrder(args),
                    "edit_purchase_order" => await EditPurchaseOrder(args),
                    "edit_warehouse" => await EditWarehouse(args),
                    "edit_branch" => await EditBranch(args),
                    "edit_shipment" => await EditShipment(args),
                    "edit_invoice" => await EditInvoice(args),
                    "edit_bill" => await EditBill(args),
                    "edit_goods_received_note" => await EditGoodsReceivedNote(args),
                    "edit_payment_voucher" => await EditPaymentVoucher(args),
                    "edit_payment_receive" => await EditPaymentReceive(args),
                    "edit_cash_bank" => await EditCashBank(args),
                    "edit_currency" => await EditCurrency(args),
                    "edit_bill_type" => await EditBillType(args),
                    "edit_customer_type" => await EditCustomerType(args),
                    "edit_invoice_type" => await EditInvoiceType(args),
                    "edit_payment_type" => await EditPaymentType(args),
                    "edit_product_type" => await EditProductType(args),
                    "edit_sales_type" => await EditSalesType(args),
                    "edit_shipment_type" => await EditShipmentType(args),
                    "edit_unit_of_measure" => await EditUnitOfMeasure(args),
                    "edit_vendor_type" => await EditVendorType(args),
                    "edit_purchase_type" => await EditPurchaseType(args),
                    "get_dashboard_data" => await GetDashboardData(),
                    _ => $"Unknown function: {functionName}"
                };
            }
            catch (Exception ex)
            {
                return $"Error executing {functionName}: {ex.Message}";
            }
        }

        #region Helpers

        private static string BuildSystemPrompt(IList<string> userRoles)
        {
            var sb = new StringBuilder(BaseSystemPrompt);
            sb.Append("\n\n--- USER PERMISSIONS ---\n");
            sb.Append("IMPORTANT: The current user has the following roles: ");

            if (userRoles.Count == 0)
            {
                sb.Append("NONE.");
                sb.Append("\nThe user has NO permissions to create, edit, delete, or view any specific module data. ");
                sb.Append("If they ask to perform any such action, politely inform them that they do not have the required role and should contact an administrator.");
            }
            else
            {
                sb.Append(string.Join(", ", userRoles));
                sb.Append(".\nYou MUST only use tools that correspond to the user's roles. ");
                sb.Append("If the user asks you to perform an action on a module they do NOT have a role for, ");
                sb.Append("you MUST refuse and explain which role they would need. ");
                sb.Append("NEVER attempt to call a tool the user is not authorized to use.");
            }

            return sb.ToString();
        }

        private static List<object> GetAllowedTools(IList<string> userRoles) => [.. AllToolDefinitions.Where(tool =>
            {
                if (tool is Dictionary<string, object> toolDict
                    && toolDict.TryGetValue("function", out var funcObj)
                    && funcObj is Dictionary<string, object> funcDict
                    && funcDict.TryGetValue("name", out var nameObj)
                    && nameObj is string functionName)
                {
                    if (FunctionRoleMap.TryGetValue(functionName, out var requiredRole))
                    {
                        return requiredRole == null || userRoles.Contains(requiredRole);
                    }
                    return false;
                }
                return false;
            })];

        private static string GetStr(JsonElement args, string name) => args.TryGetProperty(name, out JsonElement val) && val.ValueKind == JsonValueKind.String ? val.GetString() ?? "" : "";

        private static int GetInt(JsonElement args, string name)
        {
            if (!args.TryGetProperty(name, out JsonElement val)) return 0;
            if (val.ValueKind == JsonValueKind.Number) return val.GetInt32();
            if (val.ValueKind == JsonValueKind.String && int.TryParse(val.GetString(), out var parsed)) return parsed;
            return 0;
        }

        private static double GetDbl(JsonElement args, string name)
        {
            if (!args.TryGetProperty(name, out JsonElement val)) return 0.0;
            if (val.ValueKind == JsonValueKind.Number) return val.GetDouble();
            if (val.ValueKind == JsonValueKind.String && double.TryParse(val.GetString(), out var parsed)) return parsed;
            return 0.0;
        }

        private static string Serialize(object obj) => JsonSerializer.Serialize(obj, SerializerOptions);

        private static void TryUpdateStr(JsonElement args, string propName, Action<string> setter, List<string> changes, string displayName)
        {
            if (args.TryGetProperty(propName, out var val) && val.ValueKind == JsonValueKind.String)
            { setter(val.GetString() ?? ""); changes.Add(displayName); }
        }

        private static void TryUpdateInt(JsonElement args, string propName, Action<int> setter, List<string> changes, string displayName)
        {
            if (!args.TryGetProperty(propName, out var val)) return;
            if (val.ValueKind == JsonValueKind.Number) { setter(val.GetInt32()); changes.Add(displayName); }
            else if (val.ValueKind == JsonValueKind.String && int.TryParse(val.GetString(), out var p)) { setter(p); changes.Add(displayName); }
        }

        private static void TryUpdateDbl(JsonElement args, string propName, Action<double> setter, List<string> changes, string displayName)
        {
            if (!args.TryGetProperty(propName, out var val)) return;
            if (val.ValueKind == JsonValueKind.Number) { setter(val.GetDouble()); changes.Add(displayName); }
            else if (val.ValueKind == JsonValueKind.String && double.TryParse(val.GetString(), out var p)) { setter(p); changes.Add(displayName); }
        }

        private static void TryUpdateBool(JsonElement args, string propName, Action<bool> setter, List<string> changes, string displayName)
        {
            if (!args.TryGetProperty(propName, out var val)) return;
            if (val.ValueKind == JsonValueKind.True) { setter(true); changes.Add(displayName); }
            else if (val.ValueKind == JsonValueKind.False) { setter(false); changes.Add(displayName); }
        }

        #endregion

        #region Function Implementations

        private async Task<string> GetInventorySummary()
        {
            var summary = new
            {
                Products = await _context.Product.CountAsync(),
                Customers = await _context.Customer.CountAsync(),
                Vendors = await _context.Vendor.CountAsync(),
                SalesOrders = await _context.SalesOrder.CountAsync(),
                PurchaseOrders = await _context.PurchaseOrder.CountAsync(),
                Shipments = await _context.Shipment.CountAsync(),
                Invoices = await _context.Invoice.CountAsync(),
                GoodsReceivedNotes = await _context.GoodsReceivedNote.CountAsync(),
                Bills = await _context.Bill.CountAsync(),
                PaymentReceives = await _context.PaymentReceive.CountAsync(),
                PaymentVouchers = await _context.PaymentVoucher.CountAsync(),
                Warehouses = await _context.Warehouse.CountAsync(),
                Branches = await _context.Branch.CountAsync()
            };
            return Serialize(summary);
        }

        private async Task<string> ListProducts()
        {
            var items = await _context.Product.Take(50)
                .Select(p => new { p.ProductId, p.ProductName, p.ProductCode, p.Description, p.DefaultBuyingPrice, p.DefaultSellingPrice })
                .ToListAsync();
            return Serialize(new { items.Count, Items = items });
        }

        private async Task<string> SearchProducts(string query)
        {
            var items = await _context.Product
                .Where(p => p.ProductName.Contains(query) || p.ProductCode.Contains(query))
                .Take(50)
                .Select(p => new { p.ProductId, p.ProductName, p.ProductCode, p.Description, p.DefaultBuyingPrice, p.DefaultSellingPrice })
                .ToListAsync();
            return Serialize(new { items.Count, Items = items });
        }

        private async Task<string> CreateProduct(JsonElement args)
        {
            var product = new Product
            {
                ProductName = GetStr(args, "productName"),
                Description = GetStr(args, "description"),
                DefaultBuyingPrice = GetDbl(args, "defaultBuyingPrice"),
                DefaultSellingPrice = GetDbl(args, "defaultSellingPrice"),
                UnitOfMeasureId = await _context.UnitOfMeasure.Select(u => u.UnitOfMeasureId).FirstOrDefaultAsync(),
                BranchId = await _context.Branch.Select(b => b.BranchId).FirstOrDefaultAsync(),
                CurrencyId = await _context.Currency.Select(c => c.CurrencyId).FirstOrDefaultAsync()
            };
            _context.Product.Add(product);
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Product '{product.ProductName}' created with ID {product.ProductId}.", product.ProductId, product.ProductName });
        }

        private async Task<string> DeleteProduct(int id)
        {
            Product product = await _context.Product.FindAsync(id);
            if (product == null) return Serialize(new { Success = false, Message = $"Product with ID {id} not found." });
            _context.Product.Remove(product);
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Product '{product.ProductName}' (ID {id}) deleted." });
        }

        private async Task<string> ListCustomers()
        {
            var items = await _context.Customer.Take(50)
                .Select(c => new { c.CustomerId, c.CustomerName, c.Address, c.City, c.Phone, c.Email, c.ContactPerson })
                .ToListAsync();
            return Serialize(new { items.Count, Items = items });
        }

        private async Task<string> SearchCustomers(string query)
        {
            var items = await _context.Customer
                .Where(c => c.CustomerName.Contains(query))
                .Take(50)
                .Select(c => new { c.CustomerId, c.CustomerName, c.Address, c.City, c.Phone, c.Email })
                .ToListAsync();
            return Serialize(new { items.Count, Items = items });
        }

        private async Task<string> CreateCustomer(JsonElement args)
        {
            var customer = new Customer
            {
                CustomerName = GetStr(args, "customerName"),
                Address = GetStr(args, "address"),
                City = GetStr(args, "city"),
                State = GetStr(args, "state"),
                ZipCode = GetStr(args, "zipCode"),
                Phone = GetStr(args, "phone"),
                Email = GetStr(args, "email"),
                ContactPerson = GetStr(args, "contactPerson"),
                CustomerTypeId = await _context.CustomerType.Select(ct => ct.CustomerTypeId).FirstOrDefaultAsync()
            };
            _context.Customer.Add(customer);
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Customer '{customer.CustomerName}' created with ID {customer.CustomerId}.", customer.CustomerId, customer.CustomerName });
        }

        private async Task<string> DeleteCustomer(int id)
        {
            Customer customer = await _context.Customer.FindAsync(id);
            if (customer == null) return Serialize(new { Success = false, Message = $"Customer with ID {id} not found." });
            _context.Customer.Remove(customer);
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Customer '{customer.CustomerName}' (ID {id}) deleted." });
        }

        private async Task<string> ListVendors()
        {
            var items = await _context.Vendor.Take(50)
                .Select(v => new { v.VendorId, v.VendorName, v.Address, v.City, v.Phone, v.Email, v.ContactPerson })
                .ToListAsync();
            return Serialize(new { items.Count, Items = items });
        }

        private async Task<string> SearchVendors(string query)
        {
            var items = await _context.Vendor
                .Where(v => v.VendorName.Contains(query))
                .Take(50)
                .Select(v => new { v.VendorId, v.VendorName, v.Address, v.City, v.Phone, v.Email })
                .ToListAsync();
            return Serialize(new { items.Count, Items = items });
        }

        private async Task<string> CreateVendor(JsonElement args)
        {
            var vendor = new Vendor
            {
                VendorName = GetStr(args, "vendorName"),
                Address = GetStr(args, "address"),
                City = GetStr(args, "city"),
                State = GetStr(args, "state"),
                ZipCode = GetStr(args, "zipCode"),
                Phone = GetStr(args, "phone"),
                Email = GetStr(args, "email"),
                ContactPerson = GetStr(args, "contactPerson"),
                VendorTypeId = await _context.VendorType.Select(vt => vt.VendorTypeId).FirstOrDefaultAsync()
            };
            _context.Vendor.Add(vendor);
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Vendor '{vendor.VendorName}' created with ID {vendor.VendorId}.", vendor.VendorId, vendor.VendorName });
        }

        private async Task<string> DeleteVendor(int id)
        {
            Vendor vendor = await _context.Vendor.FindAsync(id);
            if (vendor == null) return Serialize(new { Success = false, Message = $"Vendor with ID {id} not found." });
            _context.Vendor.Remove(vendor);
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Vendor '{vendor.VendorName}' (ID {id}) deleted." });
        }

        private async Task<string> ListSalesOrders()
        {
            var items = await _context.SalesOrder.Take(50)
                .Select(so => new { so.SalesOrderId, so.SalesOrderName, so.CustomerId, so.OrderDate, so.DeliveryDate, so.Total, so.Remarks })
                .ToListAsync();
            return Serialize(new { items.Count, Items = items });
        }

        private async Task<string> SearchSalesOrders(string query)
        {
            var items = await _context.SalesOrder
                .Where(so => so.SalesOrderName.Contains(query) || so.Remarks.Contains(query))
                .Take(50)
                .Select(so => new { so.SalesOrderId, so.SalesOrderName, so.CustomerId, so.OrderDate, so.DeliveryDate, so.Total, so.Remarks })
                .ToListAsync();
            return Serialize(new { items.Count, Items = items });
        }

        private async Task<string> CreateSalesOrder(JsonElement args)
        {
            var salesOrder = new SalesOrder
            {
                SalesOrderName = _numberSequence.GetNumberSequence("SO"),
                CustomerId = GetInt(args, "customerId"),
                Remarks = GetStr(args, "remarks"),
                OrderDate = DateTimeOffset.Now,
                DeliveryDate = DateTimeOffset.Now.AddDays(7),
                BranchId = await _context.Branch.Select(b => b.BranchId).FirstOrDefaultAsync(),
                CurrencyId = await _context.Currency.Select(c => c.CurrencyId).FirstOrDefaultAsync(),
                SalesTypeId = await _context.SalesType.Select(st => st.SalesTypeId).FirstOrDefaultAsync()
            };
            _context.SalesOrder.Add(salesOrder);
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Sales Order '{salesOrder.SalesOrderName}' created with ID {salesOrder.SalesOrderId}.", salesOrder.SalesOrderId, salesOrder.SalesOrderName });
        }

        private async Task<string> DeleteSalesOrder(int id)
        {
            SalesOrder order = await _context.SalesOrder.FindAsync(id);
            if (order == null) return Serialize(new { Success = false, Message = $"Sales Order with ID {id} not found." });
            _context.SalesOrder.Remove(order);
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Sales Order '{order.SalesOrderName}' (ID {id}) deleted." });
        }

        private async Task<string> ListPurchaseOrders()
        {
            var items = await _context.PurchaseOrder.Take(50)
                .Select(po => new { po.PurchaseOrderId, po.PurchaseOrderName, po.VendorId, po.OrderDate, po.DeliveryDate, po.Total, po.Remarks })
                .ToListAsync();
            return Serialize(new { items.Count, Items = items });
        }

        private async Task<string> SearchPurchaseOrders(string query)
        {
            var items = await _context.PurchaseOrder
                .Where(po => po.PurchaseOrderName.Contains(query) || po.Remarks.Contains(query))
                .Take(50)
                .Select(po => new { po.PurchaseOrderId, po.PurchaseOrderName, po.VendorId, po.OrderDate, po.DeliveryDate, po.Total, po.Remarks })
                .ToListAsync();
            return Serialize(new { items.Count, Items = items });
        }

        private async Task<string> CreatePurchaseOrder(JsonElement args)
        {
            var purchaseOrder = new PurchaseOrder
            {
                PurchaseOrderName = _numberSequence.GetNumberSequence("PO"),
                VendorId = GetInt(args, "vendorId"),
                Remarks = GetStr(args, "remarks"),
                OrderDate = DateTimeOffset.Now,
                DeliveryDate = DateTimeOffset.Now.AddDays(14),
                BranchId = await _context.Branch.Select(b => b.BranchId).FirstOrDefaultAsync(),
                CurrencyId = await _context.Currency.Select(c => c.CurrencyId).FirstOrDefaultAsync(),
                PurchaseTypeId = await _context.PurchaseType.Select(pt => pt.PurchaseTypeId).FirstOrDefaultAsync()
            };
            _context.PurchaseOrder.Add(purchaseOrder);
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Purchase Order '{purchaseOrder.PurchaseOrderName}' created with ID {purchaseOrder.PurchaseOrderId}.", purchaseOrder.PurchaseOrderId, purchaseOrder.PurchaseOrderName });
        }

        private async Task<string> DeletePurchaseOrder(int id)
        {
            PurchaseOrder order = await _context.PurchaseOrder.FindAsync(id);
            if (order == null) return Serialize(new { Success = false, Message = $"Purchase Order with ID {id} not found." });
            _context.PurchaseOrder.Remove(order);
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Purchase Order '{order.PurchaseOrderName}' (ID {id}) deleted." });
        }

        private async Task<string> ListWarehouses()
        {
            var items = await _context.Warehouse.Select(w => new { w.WarehouseId, w.WarehouseName, w.Description, w.BranchId }).ToListAsync();
            return Serialize(new { items.Count, Items = items });
        }

        private async Task<string> SearchWarehouses(string query)
        {
            var items = await _context.Warehouse
                .Where(w => w.WarehouseName.Contains(query))
                .Select(w => new { w.WarehouseId, w.WarehouseName, w.Description, w.BranchId })
                .ToListAsync();
            return Serialize(new { items.Count, Items = items });
        }

        private async Task<string> CreateWarehouse(JsonElement args)
        {
            var warehouse = new Warehouse
            {
                WarehouseName = GetStr(args, "warehouseName"),
                Description = GetStr(args, "description"),
                BranchId = GetInt(args, "branchId") is var bid and > 0 ? bid : await _context.Branch.Select(b => b.BranchId).FirstOrDefaultAsync()
            };
            _context.Warehouse.Add(warehouse);
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Warehouse '{warehouse.WarehouseName}' created with ID {warehouse.WarehouseId}.", warehouse.WarehouseId, warehouse.WarehouseName });
        }

        private async Task<string> DeleteWarehouse(int id)
        {
            Warehouse warehouse = await _context.Warehouse.FindAsync(id);
            if (warehouse == null) return Serialize(new { Success = false, Message = $"Warehouse with ID {id} not found." });
            _context.Warehouse.Remove(warehouse);
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Warehouse '{warehouse.WarehouseName}' (ID {id}) deleted." });
        }

        private async Task<string> ListBranches()
        {
            var items = await _context.Branch.Select(b => new { b.BranchId, b.BranchName, b.Description, b.Address, b.City, b.State, b.Phone, b.Email, b.ContactPerson }).ToListAsync();
            return Serialize(new { items.Count, Items = items });
        }

        private async Task<string> SearchBranches(string query)
        {
            var items = await _context.Branch
                .Where(b => b.BranchName.Contains(query))
                .Select(b => new { b.BranchId, b.BranchName, b.Description, b.Address, b.City, b.Phone, b.Email })
                .ToListAsync();
            return Serialize(new { items.Count, Items = items });
        }

        private async Task<string> CreateBranch(JsonElement args)
        {
            var branch = new Branch
            {
                BranchName = GetStr(args, "branchName"),
                Description = GetStr(args, "description"),
                Address = GetStr(args, "address"),
                City = GetStr(args, "city"),
                State = GetStr(args, "state"),
                ZipCode = GetStr(args, "zipCode"),
                Phone = GetStr(args, "phone"),
                Email = GetStr(args, "email"),
                ContactPerson = GetStr(args, "contactPerson"),
                CurrencyId = GetInt(args, "currencyId") is var cid and > 0 ? cid : await _context.Currency.Select(c => c.CurrencyId).FirstOrDefaultAsync()
            };
            _context.Branch.Add(branch);
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Branch '{branch.BranchName}' created with ID {branch.BranchId}.", branch.BranchId, branch.BranchName });
        }

        private async Task<string> DeleteBranch(int id)
        {
            Branch branch = await _context.Branch.FindAsync(id);
            if (branch == null) return Serialize(new { Success = false, Message = $"Branch with ID {id} not found." });
            _context.Branch.Remove(branch);
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Branch '{branch.BranchName}' (ID {id}) deleted." });
        }

        private async Task<string> ListShipments()
        {
            var items = await _context.Shipment.Take(50)
                .Select(s => new { s.ShipmentId, s.ShipmentName, s.SalesOrderId, s.ShipmentDate, s.IsFullShipment, s.ShipmentTypeId, s.WarehouseId })
                .ToListAsync();
            return Serialize(new { items.Count, Items = items });
        }

        private async Task<string> SearchShipments(string query)
        {
            var items = await _context.Shipment
                .Where(s => s.ShipmentName.Contains(query))
                .Take(50)
                .Select(s => new { s.ShipmentId, s.ShipmentName, s.SalesOrderId, s.ShipmentDate, s.IsFullShipment })
                .ToListAsync();
            return Serialize(new { items.Count, Items = items });
        }

        private async Task<string> CreateShipment(JsonElement args)
        {
            var shipment = new Shipment
            {
                ShipmentName = _numberSequence.GetNumberSequence("SHP"),
                SalesOrderId = GetInt(args, "salesOrderId"),
                ShipmentDate = DateTimeOffset.Now,
                IsFullShipment = args.TryGetProperty("isFullShipment", out JsonElement fs) && fs.ValueKind == JsonValueKind.False ? false : true,
                ShipmentTypeId = GetInt(args, "shipmentTypeId") is var stid and > 0 ? stid : await _context.ShipmentType.Select(st => st.ShipmentTypeId).FirstOrDefaultAsync(),
                WarehouseId = GetInt(args, "warehouseId") is var wid and > 0 ? wid : await _context.Warehouse.Select(w => w.WarehouseId).FirstOrDefaultAsync()
            };
            _context.Shipment.Add(shipment);
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Shipment '{shipment.ShipmentName}' created with ID {shipment.ShipmentId} for Sales Order {shipment.SalesOrderId}.", shipment.ShipmentId, shipment.ShipmentName });
        }

        private async Task<string> DeleteShipment(int id)
        {
            Shipment shipment = await _context.Shipment.FindAsync(id);
            if (shipment == null) return Serialize(new { Success = false, Message = $"Shipment with ID {id} not found." });
            _context.Shipment.Remove(shipment);
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Shipment '{shipment.ShipmentName}' (ID {id}) deleted." });
        }

        private async Task<string> ListInvoices()
        {
            var items = await _context.Invoice.Take(50)
                .Select(i => new { i.InvoiceId, i.InvoiceName, i.ShipmentId, i.InvoiceDate, i.InvoiceDueDate, i.InvoiceTypeId })
                .ToListAsync();
            return Serialize(new { items.Count, Items = items });
        }

        private async Task<string> SearchInvoices(string query)
        {
            var items = await _context.Invoice
                .Where(i => i.InvoiceName.Contains(query))
                .Take(50)
                .Select(i => new { i.InvoiceId, i.InvoiceName, i.ShipmentId, i.InvoiceDate, i.InvoiceDueDate })
                .ToListAsync();
            return Serialize(new { items.Count, Items = items });
        }

        private async Task<string> CreateInvoice(JsonElement args)
        {
            var invoice = new Invoice
            {
                InvoiceName = _numberSequence.GetNumberSequence("INV"),
                ShipmentId = GetInt(args, "shipmentId"),
                InvoiceDate = DateTimeOffset.Now,
                InvoiceDueDate = DateTimeOffset.Now.AddDays(30),
                InvoiceTypeId = GetInt(args, "invoiceTypeId") is var itid and > 0 ? itid : await _context.InvoiceType.Select(it => it.InvoiceTypeId).FirstOrDefaultAsync()
            };
            _context.Invoice.Add(invoice);
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Invoice '{invoice.InvoiceName}' created with ID {invoice.InvoiceId} for Shipment {invoice.ShipmentId}.", invoice.InvoiceId, invoice.InvoiceName });
        }

        private async Task<string> DeleteInvoice(int id)
        {
            Invoice invoice = await _context.Invoice.FindAsync(id);
            if (invoice == null) return Serialize(new { Success = false, Message = $"Invoice with ID {id} not found." });
            _context.Invoice.Remove(invoice);
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Invoice '{invoice.InvoiceName}' (ID {id}) deleted." });
        }

        private async Task<string> ListBills()
        {
            var items = await _context.Bill.Take(50)
                .Select(b => new { b.BillId, b.BillName, b.GoodsReceivedNoteId, b.BillDate, b.BillDueDate, b.BillTypeId, b.VendorDONumber, b.VendorInvoiceNumber })
                .ToListAsync();
            return Serialize(new { items.Count, Items = items });
        }

        private async Task<string> SearchBills(string query)
        {
            var items = await _context.Bill
                .Where(b => b.BillName.Contains(query) || b.VendorInvoiceNumber.Contains(query))
                .Take(50)
                .Select(b => new { b.BillId, b.BillName, b.GoodsReceivedNoteId, b.BillDate, b.BillDueDate })
                .ToListAsync();
            return Serialize(new { items.Count, Items = items });
        }

        private async Task<string> CreateBill(JsonElement args)
        {
            var bill = new Bill
            {
                BillName = _numberSequence.GetNumberSequence("BILL"),
                GoodsReceivedNoteId = GetInt(args, "goodsReceivedNoteId"),
                VendorDONumber = GetStr(args, "vendorDONumber"),
                VendorInvoiceNumber = GetStr(args, "vendorInvoiceNumber"),
                BillDate = DateTimeOffset.Now,
                BillDueDate = DateTimeOffset.Now.AddDays(30),
                BillTypeId = GetInt(args, "billTypeId") is var btid and > 0 ? btid : await _context.BillType.Select(bt => bt.BillTypeId).FirstOrDefaultAsync()
            };
            _context.Bill.Add(bill);
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Bill '{bill.BillName}' created with ID {bill.BillId} for GRN {bill.GoodsReceivedNoteId}.", bill.BillId, bill.BillName });
        }

        private async Task<string> DeleteBill(int id)
        {
            Bill bill = await _context.Bill.FindAsync(id);
            if (bill == null) return Serialize(new { Success = false, Message = $"Bill with ID {id} not found." });
            _context.Bill.Remove(bill);
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Bill '{bill.BillName}' (ID {id}) deleted." });
        }

        private async Task<string> ListGoodsReceivedNotes()
        {
            var items = await _context.GoodsReceivedNote.Take(50)
                .Select(g => new { g.GoodsReceivedNoteId, g.GoodsReceivedNoteName, g.PurchaseOrderId, g.GRNDate, g.IsFullReceive, g.WarehouseId, g.VendorDONumber, g.VendorInvoiceNumber })
                .ToListAsync();
            return Serialize(new { items.Count, Items = items });
        }

        private async Task<string> SearchGoodsReceivedNotes(string query)
        {
            var items = await _context.GoodsReceivedNote
                .Where(g => g.GoodsReceivedNoteName.Contains(query) || g.VendorDONumber.Contains(query))
                .Take(50)
                .Select(g => new { g.GoodsReceivedNoteId, g.GoodsReceivedNoteName, g.PurchaseOrderId, g.GRNDate, g.IsFullReceive })
                .ToListAsync();
            return Serialize(new { items.Count, Items = items });
        }

        private async Task<string> CreateGoodsReceivedNote(JsonElement args)
        {
            var grn = new GoodsReceivedNote
            {
                GoodsReceivedNoteName = _numberSequence.GetNumberSequence("GRN"),
                PurchaseOrderId = GetInt(args, "purchaseOrderId"),
                GRNDate = DateTimeOffset.Now,
                VendorDONumber = GetStr(args, "vendorDONumber"),
                VendorInvoiceNumber = GetStr(args, "vendorInvoiceNumber"),
                IsFullReceive = !(args.TryGetProperty("isFullReceive", out JsonElement fr) && fr.ValueKind == JsonValueKind.False),
                WarehouseId = GetInt(args, "warehouseId") is var wid2 and > 0 ? wid2 : await _context.Warehouse.Select(w => w.WarehouseId).FirstOrDefaultAsync()
            };
            _context.GoodsReceivedNote.Add(grn);
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"GRN '{grn.GoodsReceivedNoteName}' created with ID {grn.GoodsReceivedNoteId} for Purchase Order {grn.PurchaseOrderId}.", grn.GoodsReceivedNoteId, grn.GoodsReceivedNoteName });
        }

        private async Task<string> DeleteGoodsReceivedNote(int id)
        {
            GoodsReceivedNote grn = await _context.GoodsReceivedNote.FindAsync(id);
            if (grn == null) return Serialize(new { Success = false, Message = $"Goods Received Note with ID {id} not found." });
            _context.GoodsReceivedNote.Remove(grn);
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"GRN '{grn.GoodsReceivedNoteName}' (ID {id}) deleted." });
        }

        private async Task<string> ListPaymentVouchers()
        {
            var items = await _context.PaymentVoucher.Take(50)
                .Select(pv => new { pv.PaymentvoucherId, pv.PaymentVoucherName, pv.BillId, pv.PaymentDate, pv.PaymentAmount, pv.IsFullPayment, pv.PaymentTypeId, pv.CashBankId })
                .ToListAsync();
            return Serialize(new { items.Count, Items = items });
        }

        private async Task<string> SearchPaymentVouchers(string query)
        {
            var items = await _context.PaymentVoucher
                .Where(pv => pv.PaymentVoucherName.Contains(query))
                .Take(50)
                .Select(pv => new { pv.PaymentvoucherId, pv.PaymentVoucherName, pv.BillId, pv.PaymentDate, pv.PaymentAmount, pv.IsFullPayment })
                .ToListAsync();
            return Serialize(new { items.Count, Items = items });
        }

        private async Task<string> CreatePaymentVoucher(JsonElement args)
        {
            var pv = new PaymentVoucher
            {
                PaymentVoucherName = _numberSequence.GetNumberSequence("PV"),
                BillId = GetInt(args, "billId"),
                PaymentDate = DateTimeOffset.Now,
                PaymentAmount = GetDbl(args, "paymentAmount"),
                IsFullPayment = !(args.TryGetProperty("isFullPayment", out JsonElement fp) && fp.ValueKind == JsonValueKind.False),
                PaymentTypeId = GetInt(args, "paymentTypeId") is var ptid and > 0 ? ptid : await _context.PaymentType.Select(pt => pt.PaymentTypeId).FirstOrDefaultAsync(),
                CashBankId = GetInt(args, "cashBankId") is var cbid and > 0 ? cbid : await _context.CashBank.Select(cb => cb.CashBankId).FirstOrDefaultAsync()
            };
            _context.PaymentVoucher.Add(pv);
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Payment Voucher '{pv.PaymentVoucherName}' created with ID {pv.PaymentvoucherId} for Bill {pv.BillId}.", pv.PaymentvoucherId, pv.PaymentVoucherName });
        }

        private async Task<string> DeletePaymentVoucher(int id)
        {
            PaymentVoucher pv = await _context.PaymentVoucher.FindAsync(id);
            if (pv == null) return Serialize(new { Success = false, Message = $"Payment Voucher with ID {id} not found." });
            _context.PaymentVoucher.Remove(pv);
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Payment Voucher '{pv.PaymentVoucherName}' (ID {id}) deleted." });
        }

        private async Task<string> ListPaymentReceives()
        {
            var items = await _context.PaymentReceive.Take(50)
                .Select(pr => new { pr.PaymentReceiveId, pr.PaymentReceiveName, pr.InvoiceId, pr.PaymentDate, pr.PaymentAmount, pr.IsFullPayment, pr.PaymentTypeId })
                .ToListAsync();
            return Serialize(new { items.Count, Items = items });
        }

        private async Task<string> SearchPaymentReceives(string query)
        {
            var items = await _context.PaymentReceive
                .Where(pr => pr.PaymentReceiveName.Contains(query))
                .Take(50)
                .Select(pr => new { pr.PaymentReceiveId, pr.PaymentReceiveName, pr.InvoiceId, pr.PaymentDate, pr.PaymentAmount, pr.IsFullPayment })
                .ToListAsync();
            return Serialize(new { items.Count, Items = items });
        }

        private async Task<string> CreatePaymentReceive(JsonElement args)
        {
            var pr = new PaymentReceive
            {
                PaymentReceiveName = _numberSequence.GetNumberSequence("PR"),
                InvoiceId = GetInt(args, "invoiceId"),
                PaymentDate = DateTimeOffset.Now,
                PaymentAmount = GetDbl(args, "paymentAmount"),
                IsFullPayment = !(args.TryGetProperty("isFullPayment", out JsonElement fp2) && fp2.ValueKind == JsonValueKind.False),
                PaymentTypeId = GetInt(args, "paymentTypeId") is var ptid2 and > 0 ? ptid2 : await _context.PaymentType.Select(pt => pt.PaymentTypeId).FirstOrDefaultAsync()
            };
            _context.PaymentReceive.Add(pr);
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Payment Receive '{pr.PaymentReceiveName}' created with ID {pr.PaymentReceiveId} for Invoice {pr.InvoiceId}.", pr.PaymentReceiveId, pr.PaymentReceiveName });
        }

        private async Task<string> DeletePaymentReceive(int id)
        {
            PaymentReceive pr = await _context.PaymentReceive.FindAsync(id);
            if (pr == null) return Serialize(new { Success = false, Message = $"Payment Receive with ID {id} not found." });
            _context.PaymentReceive.Remove(pr);
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Payment Receive '{pr.PaymentReceiveName}' (ID {id}) deleted." });
        }

        private async Task<string> ListCashBanks()
        {
            var items = await _context.CashBank.Select(cb => new { cb.CashBankId, cb.CashBankName, cb.Description }).ToListAsync();
            return Serialize(new { items.Count, Items = items });
        }

        private async Task<string> SearchCashBanks(string query)
        {
            var items = await _context.CashBank
                .Where(cb => cb.CashBankName.Contains(query))
                .Select(cb => new { cb.CashBankId, cb.CashBankName, cb.Description })
                .ToListAsync();
            return Serialize(new { items.Count, Items = items });
        }

        private async Task<string> CreateCashBank(JsonElement args)
        {
            var cashBank = new CashBank { CashBankName = GetStr(args, "cashBankName"), Description = GetStr(args, "description") };
            _context.CashBank.Add(cashBank);
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Cash Bank '{cashBank.CashBankName}' created with ID {cashBank.CashBankId}.", cashBank.CashBankId, cashBank.CashBankName });
        }

        private async Task<string> DeleteCashBank(int id)
        {
            CashBank cashBank = await _context.CashBank.FindAsync(id);
            if (cashBank == null) return Serialize(new { Success = false, Message = $"Cash Bank with ID {id} not found." });
            _context.CashBank.Remove(cashBank);
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Cash Bank '{cashBank.CashBankName}' (ID {id}) deleted." });
        }

        private async Task<string> ListCurrencies()
        {
            var items = await _context.Currency.Select(c => new { c.CurrencyId, c.CurrencyName, c.CurrencyCode, c.Description }).ToListAsync();
            return Serialize(new { items.Count, Items = items });
        }

        private async Task<string> SearchCurrencies(string query)
        {
            var items = await _context.Currency
                .Where(c => c.CurrencyName.Contains(query) || c.CurrencyCode.Contains(query))
                .Select(c => new { c.CurrencyId, c.CurrencyName, c.CurrencyCode, c.Description })
                .ToListAsync();
            return Serialize(new { items.Count, Items = items });
        }

        private async Task<string> CreateCurrency(JsonElement args)
        {
            var currency = new Currency { CurrencyName = GetStr(args, "currencyName"), CurrencyCode = GetStr(args, "currencyCode"), Description = GetStr(args, "description") };
            _context.Currency.Add(currency);
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Currency '{currency.CurrencyName}' created with ID {currency.CurrencyId}.", currency.CurrencyId, currency.CurrencyName });
        }

        private async Task<string> DeleteCurrency(int id)
        {
            Currency currency = await _context.Currency.FindAsync(id);
            if (currency == null) return Serialize(new { Success = false, Message = $"Currency with ID {id} not found." });
            _context.Currency.Remove(currency);
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Currency '{currency.CurrencyName}' (ID {id}) deleted." });
        }

        private async Task<string> ListBillTypes()
        {
            var items = await _context.BillType.Select(bt => new { bt.BillTypeId, bt.BillTypeName, bt.Description }).ToListAsync();
            return Serialize(new { items.Count, Items = items });
        }

        private async Task<string> SearchBillTypes(string query)
        {
            var items = await _context.BillType.Where(bt => bt.BillTypeName.Contains(query)).Select(bt => new { bt.BillTypeId, bt.BillTypeName, bt.Description }).ToListAsync();
            return Serialize(new { items.Count, Items = items });
        }

        private async Task<string> CreateBillType(JsonElement args)
        {
            var bt = new BillType { BillTypeName = GetStr(args, "billTypeName"), Description = GetStr(args, "description") };
            _context.BillType.Add(bt);
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Bill Type '{bt.BillTypeName}' created with ID {bt.BillTypeId}.", bt.BillTypeId, bt.BillTypeName });
        }

        private async Task<string> DeleteBillType(int id)
        {
            BillType bt = await _context.BillType.FindAsync(id);
            if (bt == null) return Serialize(new { Success = false, Message = $"Bill Type with ID {id} not found." });
            _context.BillType.Remove(bt);
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Bill Type '{bt.BillTypeName}' (ID {id}) deleted." });
        }

        private async Task<string> ListCustomerTypes()
        {
            var items = await _context.CustomerType.Select(ct => new { ct.CustomerTypeId, ct.CustomerTypeName, ct.Description }).ToListAsync();
            return Serialize(new { items.Count, Items = items });
        }

        private async Task<string> SearchCustomerTypes(string query)
        {
            var items = await _context.CustomerType.Where(ct => ct.CustomerTypeName.Contains(query)).Select(ct => new { ct.CustomerTypeId, ct.CustomerTypeName, ct.Description }).ToListAsync();
            return Serialize(new { items.Count, Items = items });
        }

        private async Task<string> CreateCustomerType(JsonElement args)
        {
            var ct = new CustomerType { CustomerTypeName = GetStr(args, "customerTypeName"), Description = GetStr(args, "description") };
            _context.CustomerType.Add(ct);
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Customer Type '{ct.CustomerTypeName}' created with ID {ct.CustomerTypeId}.", ct.CustomerTypeId, ct.CustomerTypeName });
        }

        private async Task<string> DeleteCustomerType(int id)
        {
            CustomerType ct = await _context.CustomerType.FindAsync(id);
            if (ct == null) return Serialize(new { Success = false, Message = $"Customer Type with ID {id} not found." });
            _context.CustomerType.Remove(ct);
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Customer Type '{ct.CustomerTypeName}' (ID {id}) deleted." });
        }

        private async Task<string> ListInvoiceTypes()
        {
            var items = await _context.InvoiceType.Select(it => new { it.InvoiceTypeId, it.InvoiceTypeName, it.Description }).ToListAsync();
            return Serialize(new { items.Count, Items = items });
        }

        private async Task<string> SearchInvoiceTypes(string query)
        {
            var items = await _context.InvoiceType.Where(it => it.InvoiceTypeName.Contains(query)).Select(it => new { it.InvoiceTypeId, it.InvoiceTypeName, it.Description }).ToListAsync();
            return Serialize(new { items.Count, Items = items });
        }

        private async Task<string> CreateInvoiceType(JsonElement args)
        {
            var it = new InvoiceType { InvoiceTypeName = GetStr(args, "invoiceTypeName"), Description = GetStr(args, "description") };
            _context.InvoiceType.Add(it);
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Invoice Type '{it.InvoiceTypeName}' created with ID {it.InvoiceTypeId}.", it.InvoiceTypeId, it.InvoiceTypeName });
        }

        private async Task<string> DeleteInvoiceType(int id)
        {
            InvoiceType it = await _context.InvoiceType.FindAsync(id);
            if (it == null) return Serialize(new { Success = false, Message = $"Invoice Type with ID {id} not found." });
            _context.InvoiceType.Remove(it);
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Invoice Type '{it.InvoiceTypeName}' (ID {id}) deleted." });
        }

        private async Task<string> ListPaymentTypes()
        {
            var items = await _context.PaymentType.Select(pt => new { pt.PaymentTypeId, pt.PaymentTypeName, pt.Description }).ToListAsync();
            return Serialize(new { items.Count, Items = items });
        }

        private async Task<string> SearchPaymentTypes(string query)
        {
            var items = await _context.PaymentType.Where(pt => pt.PaymentTypeName.Contains(query)).Select(pt => new { pt.PaymentTypeId, pt.PaymentTypeName, pt.Description }).ToListAsync();
            return Serialize(new { items.Count, Items = items });
        }

        private async Task<string> CreatePaymentType(JsonElement args)
        {
            var pt = new PaymentType { PaymentTypeName = GetStr(args, "paymentTypeName"), Description = GetStr(args, "description") };
            _context.PaymentType.Add(pt);
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Payment Type '{pt.PaymentTypeName}' created with ID {pt.PaymentTypeId}.", pt.PaymentTypeId, pt.PaymentTypeName });
        }

        private async Task<string> DeletePaymentType(int id)
        {
            PaymentType pt = await _context.PaymentType.FindAsync(id);
            if (pt == null) return Serialize(new { Success = false, Message = $"Payment Type with ID {id} not found." });
            _context.PaymentType.Remove(pt);
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Payment Type '{pt.PaymentTypeName}' (ID {id}) deleted." });
        }

        private async Task<string> ListProductTypes()
        {
            var items = await _context.ProductType.Select(pt => new { pt.ProductTypeId, pt.ProductTypeName, pt.Description }).ToListAsync();
            return Serialize(new { items.Count, Items = items });
        }

        private async Task<string> SearchProductTypes(string query)
        {
            var items = await _context.ProductType.Where(pt => pt.ProductTypeName.Contains(query)).Select(pt => new { pt.ProductTypeId, pt.ProductTypeName, pt.Description }).ToListAsync();
            return Serialize(new { items.Count, Items = items });
        }

        private async Task<string> CreateProductType(JsonElement args)
        {
            var pt = new ProductType { ProductTypeName = GetStr(args, "productTypeName"), Description = GetStr(args, "description") };
            _context.ProductType.Add(pt);
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Product Type '{pt.ProductTypeName}' created with ID {pt.ProductTypeId}.", pt.ProductTypeId, pt.ProductTypeName });
        }

        private async Task<string> DeleteProductType(int id)
        {
            ProductType pt = await _context.ProductType.FindAsync(id);
            if (pt == null) return Serialize(new { Success = false, Message = $"Product Type with ID {id} not found." });
            _context.ProductType.Remove(pt);
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Product Type '{pt.ProductTypeName}' (ID {id}) deleted." });
        }

        private async Task<string> ListSalesTypes()
        {
            var items = await _context.SalesType.Select(st => new { st.SalesTypeId, st.SalesTypeName, st.Description }).ToListAsync();
            return Serialize(new { items.Count, Items = items });
        }

        private async Task<string> SearchSalesTypes(string query)
        {
            var items = await _context.SalesType.Where(st => st.SalesTypeName.Contains(query)).Select(st => new { st.SalesTypeId, st.SalesTypeName, st.Description }).ToListAsync();
            return Serialize(new { items.Count, Items = items });
        }

        private async Task<string> CreateSalesType(JsonElement args)
        {
            var st = new SalesType { SalesTypeName = GetStr(args, "salesTypeName"), Description = GetStr(args, "description") };
            _context.SalesType.Add(st);
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Sales Type '{st.SalesTypeName}' created with ID {st.SalesTypeId}.", st.SalesTypeId, st.SalesTypeName });
        }

        private async Task<string> DeleteSalesType(int id)
        {
            SalesType st = await _context.SalesType.FindAsync(id);
            if (st == null) return Serialize(new { Success = false, Message = $"Sales Type with ID {id} not found." });
            _context.SalesType.Remove(st);
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Sales Type '{st.SalesTypeName}' (ID {id}) deleted." });
        }

        private async Task<string> ListShipmentTypes()
        {
            var items = await _context.ShipmentType.Select(st => new { st.ShipmentTypeId, st.ShipmentTypeName, st.Description }).ToListAsync();
            return Serialize(new { items.Count, Items = items });
        }

        private async Task<string> SearchShipmentTypes(string query)
        {
            var items = await _context.ShipmentType.Where(st => st.ShipmentTypeName.Contains(query)).Select(st => new { st.ShipmentTypeId, st.ShipmentTypeName, st.Description }).ToListAsync();
            return Serialize(new { items.Count, Items = items });
        }

        private async Task<string> CreateShipmentType(JsonElement args)
        {
            var st = new ShipmentType { ShipmentTypeName = GetStr(args, "shipmentTypeName"), Description = GetStr(args, "description") };
            _context.ShipmentType.Add(st);
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Shipment Type '{st.ShipmentTypeName}' created with ID {st.ShipmentTypeId}.", st.ShipmentTypeId, st.ShipmentTypeName });
        }

        private async Task<string> DeleteShipmentType(int id)
        {
            ShipmentType st = await _context.ShipmentType.FindAsync(id);
            if (st == null) return Serialize(new { Success = false, Message = $"Shipment Type with ID {id} not found." });
            _context.ShipmentType.Remove(st);
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Shipment Type '{st.ShipmentTypeName}' (ID {id}) deleted." });
        }

        private async Task<string> ListUnitsOfMeasure()
        {
            var items = await _context.UnitOfMeasure.Select(u => new { u.UnitOfMeasureId, u.UnitOfMeasureName, u.Description }).ToListAsync();
            return Serialize(new { items.Count, Items = items });
        }

        private async Task<string> SearchUnitsOfMeasure(string query)
        {
            var items = await _context.UnitOfMeasure.Where(u => u.UnitOfMeasureName.Contains(query)).Select(u => new { u.UnitOfMeasureId, u.UnitOfMeasureName, u.Description }).ToListAsync();
            return Serialize(new { items.Count, Items = items });
        }

        private async Task<string> CreateUnitOfMeasure(JsonElement args)
        {
            var u = new UnitOfMeasure { UnitOfMeasureName = GetStr(args, "unitOfMeasureName"), Description = GetStr(args, "description") };
            _context.UnitOfMeasure.Add(u);
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Unit of Measure '{u.UnitOfMeasureName}' created with ID {u.UnitOfMeasureId}.", u.UnitOfMeasureId, u.UnitOfMeasureName });
        }

        private async Task<string> DeleteUnitOfMeasure(int id)
        {
            UnitOfMeasure u = await _context.UnitOfMeasure.FindAsync(id);
            if (u == null) return Serialize(new { Success = false, Message = $"Unit of Measure with ID {id} not found." });
            _context.UnitOfMeasure.Remove(u);
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Unit of Measure '{u.UnitOfMeasureName}' (ID {id}) deleted." });
        }

        private async Task<string> ListVendorTypes()
        {
            var items = await _context.VendorType.Select(vt => new { vt.VendorTypeId, vt.VendorTypeName, vt.Description }).ToListAsync();
            return Serialize(new { items.Count, Items = items });
        }

        private async Task<string> SearchVendorTypes(string query)
        {
            var items = await _context.VendorType.Where(vt => vt.VendorTypeName.Contains(query)).Select(vt => new { vt.VendorTypeId, vt.VendorTypeName, vt.Description }).ToListAsync();
            return Serialize(new { items.Count, Items = items });
        }

        private async Task<string> CreateVendorType(JsonElement args)
        {
            var vt = new VendorType { VendorTypeName = GetStr(args, "vendorTypeName"), Description = GetStr(args, "description") };
            _context.VendorType.Add(vt);
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Vendor Type '{vt.VendorTypeName}' created with ID {vt.VendorTypeId}.", vt.VendorTypeId, vt.VendorTypeName });
        }

        private async Task<string> DeleteVendorType(int id)
        {
            VendorType vt = await _context.VendorType.FindAsync(id);
            if (vt == null) return Serialize(new { Success = false, Message = $"Vendor Type with ID {id} not found." });
            _context.VendorType.Remove(vt);
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Vendor Type '{vt.VendorTypeName}' (ID {id}) deleted." });
        }

        private async Task<string> ListPurchaseTypes()
        {
            var items = await _context.PurchaseType.Select(pt => new { pt.PurchaseTypeId, pt.PurchaseTypeName, pt.Description }).ToListAsync();
            return Serialize(new { items.Count, Items = items });
        }

        private async Task<string> SearchPurchaseTypes(string query)
        {
            var items = await _context.PurchaseType.Where(pt => pt.PurchaseTypeName.Contains(query)).Select(pt => new { pt.PurchaseTypeId, pt.PurchaseTypeName, pt.Description }).ToListAsync();
            return Serialize(new { items.Count, Items = items });
        }

        private async Task<string> CreatePurchaseType(JsonElement args)
        {
            var pt = new PurchaseType { PurchaseTypeName = GetStr(args, "purchaseTypeName"), Description = GetStr(args, "description") };
            _context.PurchaseType.Add(pt);
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Purchase Type '{pt.PurchaseTypeName}' created with ID {pt.PurchaseTypeId}.", pt.PurchaseTypeId, pt.PurchaseTypeName });
        }

        private async Task<string> DeletePurchaseType(int id)
        {
            PurchaseType pt = await _context.PurchaseType.FindAsync(id);
            if (pt == null) return Serialize(new { Success = false, Message = $"Purchase Type with ID {id} not found." });
            _context.PurchaseType.Remove(pt);
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Purchase Type '{pt.PurchaseTypeName}' (ID {id}) deleted." });
        }

        private async Task<string> GetDashboardData()
        {
            var data = new
            {
                TotalProducts = await _context.Product.CountAsync(),
                TotalCustomers = await _context.Customer.CountAsync(),
                TotalVendors = await _context.Vendor.CountAsync(),
                TotalSalesOrders = await _context.SalesOrder.CountAsync(),
                TotalPurchaseOrders = await _context.PurchaseOrder.CountAsync(),
                TotalShipments = await _context.Shipment.CountAsync(),
                TotalInvoices = await _context.Invoice.CountAsync(),
                TotalGoodsReceivedNotes = await _context.GoodsReceivedNote.CountAsync(),
                TotalBills = await _context.Bill.CountAsync(),
                TotalPaymentReceives = await _context.PaymentReceive.CountAsync(),
                TotalPaymentVouchers = await _context.PaymentVoucher.CountAsync(),
                TotalWarehouses = await _context.Warehouse.CountAsync(),
                TotalBranches = await _context.Branch.CountAsync(),
                TotalCashBanks = await _context.CashBank.CountAsync(),
                TotalCurrencies = await _context.Currency.CountAsync(),
                SalesOrderValue = await _context.SalesOrder.SumAsync(so => so.Total),
                PurchaseOrderValue = await _context.PurchaseOrder.SumAsync(po => po.Total)
            };
            return Serialize(data);
        }

        private async Task<string> EditProduct(JsonElement args)
        {
            var id = GetInt(args, "productId");
            var entity = await _context.Product.FindAsync(id);
            if (entity == null) return Serialize(new { Success = false, Message = $"Product with ID {id} not found." });
            var changes = new List<string>();
            TryUpdateStr(args, "productName", v => entity.ProductName = v, changes, "ProductName");
            TryUpdateStr(args, "description", v => entity.Description = v, changes, "Description");
            TryUpdateStr(args, "barcode", v => entity.Barcode = v, changes, "Barcode");
            TryUpdateDbl(args, "defaultBuyingPrice", v => entity.DefaultBuyingPrice = v, changes, "DefaultBuyingPrice");
            TryUpdateDbl(args, "defaultSellingPrice", v => entity.DefaultSellingPrice = v, changes, "DefaultSellingPrice");
            TryUpdateInt(args, "unitOfMeasureId", v => entity.UnitOfMeasureId = v, changes, "UnitOfMeasureId");
            TryUpdateInt(args, "branchId", v => entity.BranchId = v, changes, "BranchId");
            TryUpdateInt(args, "currencyId", v => entity.CurrencyId = v, changes, "CurrencyId");
            if (changes.Count == 0) return Serialize(new { Success = false, Message = "No fields to update were provided." });
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Product '{entity.ProductName}' (ID {id}) updated. Changed: {string.Join(", ", changes)}.", entity.ProductId, entity.ProductName });
        }

        private async Task<string> EditCustomer(JsonElement args)
        {
            var id = GetInt(args, "customerId");
            var entity = await _context.Customer.FindAsync(id);
            if (entity == null) return Serialize(new { Success = false, Message = $"Customer with ID {id} not found." });
            var changes = new List<string>();
            TryUpdateStr(args, "customerName", v => entity.CustomerName = v, changes, "CustomerName");
            TryUpdateStr(args, "address", v => entity.Address = v, changes, "Address");
            TryUpdateStr(args, "city", v => entity.City = v, changes, "City");
            TryUpdateStr(args, "state", v => entity.State = v, changes, "State");
            TryUpdateStr(args, "zipCode", v => entity.ZipCode = v, changes, "ZipCode");
            TryUpdateStr(args, "phone", v => entity.Phone = v, changes, "Phone");
            TryUpdateStr(args, "email", v => entity.Email = v, changes, "Email");
            TryUpdateStr(args, "contactPerson", v => entity.ContactPerson = v, changes, "ContactPerson");
            TryUpdateInt(args, "customerTypeId", v => entity.CustomerTypeId = v, changes, "CustomerTypeId");
            if (changes.Count == 0) return Serialize(new { Success = false, Message = "No fields to update were provided." });
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Customer '{entity.CustomerName}' (ID {id}) updated. Changed: {string.Join(", ", changes)}.", entity.CustomerId, entity.CustomerName });
        }

        private async Task<string> EditVendor(JsonElement args)
        {
            var id = GetInt(args, "vendorId");
            var entity = await _context.Vendor.FindAsync(id);
            if (entity == null) return Serialize(new { Success = false, Message = $"Vendor with ID {id} not found." });
            var changes = new List<string>();
            TryUpdateStr(args, "vendorName", v => entity.VendorName = v, changes, "VendorName");
            TryUpdateStr(args, "address", v => entity.Address = v, changes, "Address");
            TryUpdateStr(args, "city", v => entity.City = v, changes, "City");
            TryUpdateStr(args, "state", v => entity.State = v, changes, "State");
            TryUpdateStr(args, "zipCode", v => entity.ZipCode = v, changes, "ZipCode");
            TryUpdateStr(args, "phone", v => entity.Phone = v, changes, "Phone");
            TryUpdateStr(args, "email", v => entity.Email = v, changes, "Email");
            TryUpdateStr(args, "contactPerson", v => entity.ContactPerson = v, changes, "ContactPerson");
            TryUpdateInt(args, "vendorTypeId", v => entity.VendorTypeId = v, changes, "VendorTypeId");
            if (changes.Count == 0) return Serialize(new { Success = false, Message = "No fields to update were provided." });
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Vendor '{entity.VendorName}' (ID {id}) updated. Changed: {string.Join(", ", changes)}.", entity.VendorId, entity.VendorName });
        }

        private async Task<string> EditSalesOrder(JsonElement args)
        {
            var id = GetInt(args, "salesOrderId");
            var entity = await _context.SalesOrder.FindAsync(id);
            if (entity == null) return Serialize(new { Success = false, Message = $"Sales Order with ID {id} not found." });
            var changes = new List<string>();
            TryUpdateInt(args, "customerId", v => entity.CustomerId = v, changes, "CustomerId");
            TryUpdateStr(args, "remarks", v => entity.Remarks = v, changes, "Remarks");
            TryUpdateStr(args, "customerRefNumber", v => entity.CustomerRefNumber = v, changes, "CustomerRefNumber");
            TryUpdateInt(args, "branchId", v => entity.BranchId = v, changes, "BranchId");
            TryUpdateInt(args, "currencyId", v => entity.CurrencyId = v, changes, "CurrencyId");
            TryUpdateInt(args, "salesTypeId", v => entity.SalesTypeId = v, changes, "SalesTypeId");
            if (changes.Count == 0) return Serialize(new { Success = false, Message = "No fields to update were provided." });
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Sales Order '{entity.SalesOrderName}' (ID {id}) updated. Changed: {string.Join(", ", changes)}.", entity.SalesOrderId, entity.SalesOrderName });
        }

        private async Task<string> EditPurchaseOrder(JsonElement args)
        {
            var id = GetInt(args, "purchaseOrderId");
            var entity = await _context.PurchaseOrder.FindAsync(id);
            if (entity == null) return Serialize(new { Success = false, Message = $"Purchase Order with ID {id} not found." });
            var changes = new List<string>();
            TryUpdateInt(args, "vendorId", v => entity.VendorId = v, changes, "VendorId");
            TryUpdateStr(args, "remarks", v => entity.Remarks = v, changes, "Remarks");
            TryUpdateInt(args, "branchId", v => entity.BranchId = v, changes, "BranchId");
            TryUpdateInt(args, "currencyId", v => entity.CurrencyId = v, changes, "CurrencyId");
            TryUpdateInt(args, "purchaseTypeId", v => entity.PurchaseTypeId = v, changes, "PurchaseTypeId");
            if (changes.Count == 0) return Serialize(new { Success = false, Message = "No fields to update were provided." });
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Purchase Order '{entity.PurchaseOrderName}' (ID {id}) updated. Changed: {string.Join(", ", changes)}.", entity.PurchaseOrderId, entity.PurchaseOrderName });
        }

        private async Task<string> EditWarehouse(JsonElement args)
        {
            var id = GetInt(args, "warehouseId");
            var entity = await _context.Warehouse.FindAsync(id);
            if (entity == null) return Serialize(new { Success = false, Message = $"Warehouse with ID {id} not found." });
            var changes = new List<string>();
            TryUpdateStr(args, "warehouseName", v => entity.WarehouseName = v, changes, "WarehouseName");
            TryUpdateStr(args, "description", v => entity.Description = v, changes, "Description");
            TryUpdateInt(args, "branchId", v => entity.BranchId = v, changes, "BranchId");
            if (changes.Count == 0) return Serialize(new { Success = false, Message = "No fields to update were provided." });
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Warehouse '{entity.WarehouseName}' (ID {id}) updated. Changed: {string.Join(", ", changes)}.", entity.WarehouseId, entity.WarehouseName });
        }

        private async Task<string> EditBranch(JsonElement args)
        {
            var id = GetInt(args, "branchId");
            var entity = await _context.Branch.FindAsync(id);
            if (entity == null) return Serialize(new { Success = false, Message = $"Branch with ID {id} not found." });
            var changes = new List<string>();
            TryUpdateStr(args, "branchName", v => entity.BranchName = v, changes, "BranchName");
            TryUpdateStr(args, "description", v => entity.Description = v, changes, "Description");
            TryUpdateStr(args, "address", v => entity.Address = v, changes, "Address");
            TryUpdateStr(args, "city", v => entity.City = v, changes, "City");
            TryUpdateStr(args, "state", v => entity.State = v, changes, "State");
            TryUpdateStr(args, "zipCode", v => entity.ZipCode = v, changes, "ZipCode");
            TryUpdateStr(args, "phone", v => entity.Phone = v, changes, "Phone");
            TryUpdateStr(args, "email", v => entity.Email = v, changes, "Email");
            TryUpdateStr(args, "contactPerson", v => entity.ContactPerson = v, changes, "ContactPerson");
            TryUpdateInt(args, "currencyId", v => entity.CurrencyId = v, changes, "CurrencyId");
            if (changes.Count == 0) return Serialize(new { Success = false, Message = "No fields to update were provided." });
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Branch '{entity.BranchName}' (ID {id}) updated. Changed: {string.Join(", ", changes)}.", entity.BranchId, entity.BranchName });
        }

        private async Task<string> EditShipment(JsonElement args)
        {
            var id = GetInt(args, "shipmentId");
            var entity = await _context.Shipment.FindAsync(id);
            if (entity == null) return Serialize(new { Success = false, Message = $"Shipment with ID {id} not found." });
            var changes = new List<string>();
            TryUpdateInt(args, "salesOrderId", v => entity.SalesOrderId = v, changes, "SalesOrderId");
            TryUpdateBool(args, "isFullShipment", v => entity.IsFullShipment = v, changes, "IsFullShipment");
            TryUpdateInt(args, "shipmentTypeId", v => entity.ShipmentTypeId = v, changes, "ShipmentTypeId");
            TryUpdateInt(args, "warehouseId", v => entity.WarehouseId = v, changes, "WarehouseId");
            if (changes.Count == 0) return Serialize(new { Success = false, Message = "No fields to update were provided." });
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Shipment '{entity.ShipmentName}' (ID {id}) updated. Changed: {string.Join(", ", changes)}.", entity.ShipmentId, entity.ShipmentName });
        }

        private async Task<string> EditInvoice(JsonElement args)
        {
            var id = GetInt(args, "invoiceId");
            var entity = await _context.Invoice.FindAsync(id);
            if (entity == null) return Serialize(new { Success = false, Message = $"Invoice with ID {id} not found." });
            var changes = new List<string>();
            TryUpdateInt(args, "shipmentId", v => entity.ShipmentId = v, changes, "ShipmentId");
            TryUpdateInt(args, "invoiceTypeId", v => entity.InvoiceTypeId = v, changes, "InvoiceTypeId");
            if (changes.Count == 0) return Serialize(new { Success = false, Message = "No fields to update were provided." });
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Invoice '{entity.InvoiceName}' (ID {id}) updated. Changed: {string.Join(", ", changes)}.", entity.InvoiceId, entity.InvoiceName });
        }

        private async Task<string> EditBill(JsonElement args)
        {
            var id = GetInt(args, "billId");
            var entity = await _context.Bill.FindAsync(id);
            if (entity == null) return Serialize(new { Success = false, Message = $"Bill with ID {id} not found." });
            var changes = new List<string>();
            TryUpdateInt(args, "goodsReceivedNoteId", v => entity.GoodsReceivedNoteId = v, changes, "GoodsReceivedNoteId");
            TryUpdateStr(args, "vendorDONumber", v => entity.VendorDONumber = v, changes, "VendorDONumber");
            TryUpdateStr(args, "vendorInvoiceNumber", v => entity.VendorInvoiceNumber = v, changes, "VendorInvoiceNumber");
            TryUpdateInt(args, "billTypeId", v => entity.BillTypeId = v, changes, "BillTypeId");
            if (changes.Count == 0) return Serialize(new { Success = false, Message = "No fields to update were provided." });
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Bill '{entity.BillName}' (ID {id}) updated. Changed: {string.Join(", ", changes)}.", entity.BillId, entity.BillName });
        }

        private async Task<string> EditGoodsReceivedNote(JsonElement args)
        {
            var id = GetInt(args, "goodsReceivedNoteId");
            var entity = await _context.GoodsReceivedNote.FindAsync(id);
            if (entity == null) return Serialize(new { Success = false, Message = $"Goods Received Note with ID {id} not found." });
            var changes = new List<string>();
            TryUpdateInt(args, "purchaseOrderId", v => entity.PurchaseOrderId = v, changes, "PurchaseOrderId");
            TryUpdateStr(args, "vendorDONumber", v => entity.VendorDONumber = v, changes, "VendorDONumber");
            TryUpdateStr(args, "vendorInvoiceNumber", v => entity.VendorInvoiceNumber = v, changes, "VendorInvoiceNumber");
            TryUpdateBool(args, "isFullReceive", v => entity.IsFullReceive = v, changes, "IsFullReceive");
            TryUpdateInt(args, "warehouseId", v => entity.WarehouseId = v, changes, "WarehouseId");
            if (changes.Count == 0) return Serialize(new { Success = false, Message = "No fields to update were provided." });
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"GRN '{entity.GoodsReceivedNoteName}' (ID {id}) updated. Changed: {string.Join(", ", changes)}.", entity.GoodsReceivedNoteId, entity.GoodsReceivedNoteName });
        }

        private async Task<string> EditPaymentVoucher(JsonElement args)
        {
            var id = GetInt(args, "paymentVoucherId");
            var entity = await _context.PaymentVoucher.FindAsync(id);
            if (entity == null) return Serialize(new { Success = false, Message = $"Payment Voucher with ID {id} not found." });
            var changes = new List<string>();
            TryUpdateInt(args, "billId", v => entity.BillId = v, changes, "BillId");
            TryUpdateDbl(args, "paymentAmount", v => entity.PaymentAmount = v, changes, "PaymentAmount");
            TryUpdateBool(args, "isFullPayment", v => entity.IsFullPayment = v, changes, "IsFullPayment");
            TryUpdateInt(args, "paymentTypeId", v => entity.PaymentTypeId = v, changes, "PaymentTypeId");
            TryUpdateInt(args, "cashBankId", v => entity.CashBankId = v, changes, "CashBankId");
            if (changes.Count == 0) return Serialize(new { Success = false, Message = "No fields to update were provided." });
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Payment Voucher '{entity.PaymentVoucherName}' (ID {id}) updated. Changed: {string.Join(", ", changes)}.", entity.PaymentvoucherId, entity.PaymentVoucherName });
        }

        private async Task<string> EditPaymentReceive(JsonElement args)
        {
            var id = GetInt(args, "paymentReceiveId");
            var entity = await _context.PaymentReceive.FindAsync(id);
            if (entity == null) return Serialize(new { Success = false, Message = $"Payment Receive with ID {id} not found." });
            var changes = new List<string>();
            TryUpdateInt(args, "invoiceId", v => entity.InvoiceId = v, changes, "InvoiceId");
            TryUpdateDbl(args, "paymentAmount", v => entity.PaymentAmount = v, changes, "PaymentAmount");
            TryUpdateBool(args, "isFullPayment", v => entity.IsFullPayment = v, changes, "IsFullPayment");
            TryUpdateInt(args, "paymentTypeId", v => entity.PaymentTypeId = v, changes, "PaymentTypeId");
            if (changes.Count == 0) return Serialize(new { Success = false, Message = "No fields to update were provided." });
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Payment Receive '{entity.PaymentReceiveName}' (ID {id}) updated. Changed: {string.Join(", ", changes)}.", entity.PaymentReceiveId, entity.PaymentReceiveName });
        }

        private async Task<string> EditCashBank(JsonElement args)
        {
            var id = GetInt(args, "cashBankId");
            var entity = await _context.CashBank.FindAsync(id);
            if (entity == null) return Serialize(new { Success = false, Message = $"Cash Bank with ID {id} not found." });
            var changes = new List<string>();
            TryUpdateStr(args, "cashBankName", v => entity.CashBankName = v, changes, "CashBankName");
            TryUpdateStr(args, "description", v => entity.Description = v, changes, "Description");
            if (changes.Count == 0) return Serialize(new { Success = false, Message = "No fields to update were provided." });
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Cash Bank '{entity.CashBankName}' (ID {id}) updated. Changed: {string.Join(", ", changes)}.", entity.CashBankId, entity.CashBankName });
        }

        private async Task<string> EditCurrency(JsonElement args)
        {
            var id = GetInt(args, "currencyId");
            var entity = await _context.Currency.FindAsync(id);
            if (entity == null) return Serialize(new { Success = false, Message = $"Currency with ID {id} not found." });
            var changes = new List<string>();
            TryUpdateStr(args, "currencyName", v => entity.CurrencyName = v, changes, "CurrencyName");
            TryUpdateStr(args, "currencyCode", v => entity.CurrencyCode = v, changes, "CurrencyCode");
            TryUpdateStr(args, "description", v => entity.Description = v, changes, "Description");
            if (changes.Count == 0) return Serialize(new { Success = false, Message = "No fields to update were provided." });
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Currency '{entity.CurrencyName}' (ID {id}) updated. Changed: {string.Join(", ", changes)}.", entity.CurrencyId, entity.CurrencyName });
        }

        private async Task<string> EditBillType(JsonElement args)
        {
            var id = GetInt(args, "billTypeId");
            var entity = await _context.BillType.FindAsync(id);
            if (entity == null) return Serialize(new { Success = false, Message = $"Bill Type with ID {id} not found." });
            var changes = new List<string>();
            TryUpdateStr(args, "billTypeName", v => entity.BillTypeName = v, changes, "BillTypeName");
            TryUpdateStr(args, "description", v => entity.Description = v, changes, "Description");
            if (changes.Count == 0) return Serialize(new { Success = false, Message = "No fields to update were provided." });
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Bill Type '{entity.BillTypeName}' (ID {id}) updated. Changed: {string.Join(", ", changes)}.", entity.BillTypeId, entity.BillTypeName });
        }

        private async Task<string> EditCustomerType(JsonElement args)
        {
            var id = GetInt(args, "customerTypeId");
            var entity = await _context.CustomerType.FindAsync(id);
            if (entity == null) return Serialize(new { Success = false, Message = $"Customer Type with ID {id} not found." });
            var changes = new List<string>();
            TryUpdateStr(args, "customerTypeName", v => entity.CustomerTypeName = v, changes, "CustomerTypeName");
            TryUpdateStr(args, "description", v => entity.Description = v, changes, "Description");
            if (changes.Count == 0) return Serialize(new { Success = false, Message = "No fields to update were provided." });
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Customer Type '{entity.CustomerTypeName}' (ID {id}) updated. Changed: {string.Join(", ", changes)}.", entity.CustomerTypeId, entity.CustomerTypeName });
        }

        private async Task<string> EditInvoiceType(JsonElement args)
        {
            var id = GetInt(args, "invoiceTypeId");
            var entity = await _context.InvoiceType.FindAsync(id);
            if (entity == null) return Serialize(new { Success = false, Message = $"Invoice Type with ID {id} not found." });
            var changes = new List<string>();
            TryUpdateStr(args, "invoiceTypeName", v => entity.InvoiceTypeName = v, changes, "InvoiceTypeName");
            TryUpdateStr(args, "description", v => entity.Description = v, changes, "Description");
            if (changes.Count == 0) return Serialize(new { Success = false, Message = "No fields to update were provided." });
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Invoice Type '{entity.InvoiceTypeName}' (ID {id}) updated. Changed: {string.Join(", ", changes)}.", entity.InvoiceTypeId, entity.InvoiceTypeName });
        }

        private async Task<string> EditPaymentType(JsonElement args)
        {
            var id = GetInt(args, "paymentTypeId");
            var entity = await _context.PaymentType.FindAsync(id);
            if (entity == null) return Serialize(new { Success = false, Message = $"Payment Type with ID {id} not found." });
            var changes = new List<string>();
            TryUpdateStr(args, "paymentTypeName", v => entity.PaymentTypeName = v, changes, "PaymentTypeName");
            TryUpdateStr(args, "description", v => entity.Description = v, changes, "Description");
            if (changes.Count == 0) return Serialize(new { Success = false, Message = "No fields to update were provided." });
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Payment Type '{entity.PaymentTypeName}' (ID {id}) updated. Changed: {string.Join(", ", changes)}.", entity.PaymentTypeId, entity.PaymentTypeName });
        }

        private async Task<string> EditProductType(JsonElement args)
        {
            var id = GetInt(args, "productTypeId");
            var entity = await _context.ProductType.FindAsync(id);
            if (entity == null) return Serialize(new { Success = false, Message = $"Product Type with ID {id} not found." });
            var changes = new List<string>();
            TryUpdateStr(args, "productTypeName", v => entity.ProductTypeName = v, changes, "ProductTypeName");
            TryUpdateStr(args, "description", v => entity.Description = v, changes, "Description");
            if (changes.Count == 0) return Serialize(new { Success = false, Message = "No fields to update were provided." });
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Product Type '{entity.ProductTypeName}' (ID {id}) updated. Changed: {string.Join(", ", changes)}.", entity.ProductTypeId, entity.ProductTypeName });
        }

        private async Task<string> EditSalesType(JsonElement args)
        {
            var id = GetInt(args, "salesTypeId");
            var entity = await _context.SalesType.FindAsync(id);
            if (entity == null) return Serialize(new { Success = false, Message = $"Sales Type with ID {id} not found." });
            var changes = new List<string>();
            TryUpdateStr(args, "salesTypeName", v => entity.SalesTypeName = v, changes, "SalesTypeName");
            TryUpdateStr(args, "description", v => entity.Description = v, changes, "Description");
            if (changes.Count == 0) return Serialize(new { Success = false, Message = "No fields to update were provided." });
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Sales Type '{entity.SalesTypeName}' (ID {id}) updated. Changed: {string.Join(", ", changes)}.", entity.SalesTypeId, entity.SalesTypeName });
        }

        private async Task<string> EditShipmentType(JsonElement args)
        {
            var id = GetInt(args, "shipmentTypeId");
            var entity = await _context.ShipmentType.FindAsync(id);
            if (entity == null) return Serialize(new { Success = false, Message = $"Shipment Type with ID {id} not found." });
            var changes = new List<string>();
            TryUpdateStr(args, "shipmentTypeName", v => entity.ShipmentTypeName = v, changes, "ShipmentTypeName");
            TryUpdateStr(args, "description", v => entity.Description = v, changes, "Description");
            if (changes.Count == 0) return Serialize(new { Success = false, Message = "No fields to update were provided." });
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Shipment Type '{entity.ShipmentTypeName}' (ID {id}) updated. Changed: {string.Join(", ", changes)}.", entity.ShipmentTypeId, entity.ShipmentTypeName });
        }

        private async Task<string> EditUnitOfMeasure(JsonElement args)
        {
            var id = GetInt(args, "unitOfMeasureId");
            var entity = await _context.UnitOfMeasure.FindAsync(id);
            if (entity == null) return Serialize(new { Success = false, Message = $"Unit of Measure with ID {id} not found." });
            var changes = new List<string>();
            TryUpdateStr(args, "unitOfMeasureName", v => entity.UnitOfMeasureName = v, changes, "UnitOfMeasureName");
            TryUpdateStr(args, "description", v => entity.Description = v, changes, "Description");
            if (changes.Count == 0) return Serialize(new { Success = false, Message = "No fields to update were provided." });
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Unit of Measure '{entity.UnitOfMeasureName}' (ID {id}) updated. Changed: {string.Join(", ", changes)}.", entity.UnitOfMeasureId, entity.UnitOfMeasureName });
        }

        private async Task<string> EditVendorType(JsonElement args)
        {
            var id = GetInt(args, "vendorTypeId");
            var entity = await _context.VendorType.FindAsync(id);
            if (entity == null) return Serialize(new { Success = false, Message = $"Vendor Type with ID {id} not found." });
            var changes = new List<string>();
            TryUpdateStr(args, "vendorTypeName", v => entity.VendorTypeName = v, changes, "VendorTypeName");
            TryUpdateStr(args, "description", v => entity.Description = v, changes, "Description");
            if (changes.Count == 0) return Serialize(new { Success = false, Message = "No fields to update were provided." });
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Vendor Type '{entity.VendorTypeName}' (ID {id}) updated. Changed: {string.Join(", ", changes)}.", entity.VendorTypeId, entity.VendorTypeName });
        }

        private async Task<string> EditPurchaseType(JsonElement args)
        {
            var id = GetInt(args, "purchaseTypeId");
            var entity = await _context.PurchaseType.FindAsync(id);
            if (entity == null) return Serialize(new { Success = false, Message = $"Purchase Type with ID {id} not found." });
            var changes = new List<string>();
            TryUpdateStr(args, "purchaseTypeName", v => entity.PurchaseTypeName = v, changes, "PurchaseTypeName");
            TryUpdateStr(args, "description", v => entity.Description = v, changes, "Description");
            if (changes.Count == 0) return Serialize(new { Success = false, Message = "No fields to update were provided." });
            await _context.SaveChangesAsync();
            return Serialize(new { Success = true, Message = $"Purchase Type '{entity.PurchaseTypeName}' (ID {id}) updated. Changed: {string.Join(", ", changes)}.", entity.PurchaseTypeId, entity.PurchaseTypeName });
        }

        #endregion

        #region Tool Definitions

        private static List<object> BuildTools()
        {
            var tools = new List<object>();

            void AddTool(string name, string description, Dictionary<string, object> parameters = null)
            {
                tools.Add(new Dictionary<string, object>
                {
                    ["type"] = "function",
                    ["function"] = new Dictionary<string, object>
                    {
                        ["name"] = name,
                        ["description"] = description,
                        ["parameters"] = parameters ?? new Dictionary<string, object>
                        {
                            ["type"] = "object",
                            ["properties"] = new Dictionary<string, object>(),
                            ["required"] = Array.Empty<string>()
                        }
                    }
                });
            }

            Dictionary<string, object> Params(Dictionary<string, object> props, params string[] required)
            {
                return new Dictionary<string, object>
                {
                    ["type"] = "object",
                    ["properties"] = props,
                    ["required"] = required
                };
            }

            Dictionary<string, object> Str(string desc) => new() { ["type"] = "string", ["description"] = desc };
            Dictionary<string, object> Num(string desc) => new() { ["type"] = "number", ["description"] = desc };
            Dictionary<string, object> Int(string desc) => new() { ["type"] = "integer", ["description"] = desc };

            // Summary
            AddTool("get_inventory_summary", "Get a summary of all inventory data including counts of products, customers, vendors, orders, shipments, invoices, bills, etc.");

            // Products
            AddTool("list_products", "List all products in the inventory (up to 50)");
            AddTool("search_products", "Search products by name or code",
                Params(new Dictionary<string, object> { ["query"] = Str("Search term") }, "query"));
            AddTool("create_product", "Create a new product in the inventory",
                Params(new Dictionary<string, object>
                {
                    ["productName"] = Str("Name of the product"),
                    ["description"] = Str("Product description"),
                    ["defaultBuyingPrice"] = Num("Default buying price"),
                    ["defaultSellingPrice"] = Num("Default selling price")
                }, "productName"));
            AddTool("edit_product", "Edit/update an existing product by its ID. Only provided fields will be changed.",
                Params(new Dictionary<string, object>
                {
                    ["productId"] = Int("Product ID to edit"),
                    ["productName"] = Str("New name of the product"),
                    ["description"] = Str("New product description"),
                    ["barcode"] = Str("New barcode"),
                    ["defaultBuyingPrice"] = Num("New default buying price"),
                    ["defaultSellingPrice"] = Num("New default selling price"),
                    ["unitOfMeasureId"] = Int("New unit of measure ID"),
                    ["branchId"] = Int("New branch ID"),
                    ["currencyId"] = Int("New currency ID")
                }, "productId"));
            AddTool("delete_product", "Delete a product by its ID",
                Params(new Dictionary<string, object> { ["productId"] = Int("Product ID to delete") }, "productId"));

            // Customers
            AddTool("list_customers", "List all customers (up to 50)");
            AddTool("search_customers", "Search customers by name",
                Params(new Dictionary<string, object> { ["query"] = Str("Search term") }, "query"));
            AddTool("create_customer", "Create a new customer",
                Params(new Dictionary<string, object>
                {
                    ["customerName"] = Str("Name of the customer"),
                    ["address"] = Str("Street address"),
                    ["city"] = Str("City"),
                    ["state"] = Str("State"),
                    ["zipCode"] = Str("Zip code"),
                    ["phone"] = Str("Phone number"),
                    ["email"] = Str("Email address"),
                    ["contactPerson"] = Str("Contact person name")
                }, "customerName"));
            AddTool("edit_customer", "Edit/update an existing customer by ID. Only provided fields will be changed.",
                Params(new Dictionary<string, object>
                {
                    ["customerId"] = Int("Customer ID to edit"),
                    ["customerName"] = Str("New name of the customer"),
                    ["address"] = Str("New street address"),
                    ["city"] = Str("New city"),
                    ["state"] = Str("New state"),
                    ["zipCode"] = Str("New zip code"),
                    ["phone"] = Str("New phone number"),
                    ["email"] = Str("New email address"),
                    ["contactPerson"] = Str("New contact person name"),
                    ["customerTypeId"] = Int("New customer type ID")
                }, "customerId"));
            AddTool("delete_customer", "Delete a customer by ID",
                Params(new Dictionary<string, object> { ["customerId"] = Int("Customer ID to delete") }, "customerId"));

            // Vendors
            AddTool("list_vendors", "List all vendors (up to 50)");
            AddTool("search_vendors", "Search vendors by name",
                Params(new Dictionary<string, object> { ["query"] = Str("Search term") }, "query"));
            AddTool("create_vendor", "Create a new vendor",
                Params(new Dictionary<string, object>
                {
                    ["vendorName"] = Str("Name of the vendor"),
                    ["address"] = Str("Street address"),
                    ["city"] = Str("City"),
                    ["state"] = Str("State"),
                    ["zipCode"] = Str("Zip code"),
                    ["phone"] = Str("Phone number"),
                    ["email"] = Str("Email address"),
                    ["contactPerson"] = Str("Contact person name")
                }, "vendorName"));
            AddTool("edit_vendor", "Edit/update an existing vendor by ID. Only provided fields will be changed.",
                Params(new Dictionary<string, object>
                {
                    ["vendorId"] = Int("Vendor ID to edit"),
                    ["vendorName"] = Str("New name of the vendor"),
                    ["address"] = Str("New street address"),
                    ["city"] = Str("New city"),
                    ["state"] = Str("New state"),
                    ["zipCode"] = Str("New zip code"),
                    ["phone"] = Str("New phone number"),
                    ["email"] = Str("New email address"),
                    ["contactPerson"] = Str("New contact person name"),
                    ["vendorTypeId"] = Int("New vendor type ID")
                }, "vendorId"));
            AddTool("delete_vendor", "Delete a vendor by ID",
                Params(new Dictionary<string, object> { ["vendorId"] = Int("Vendor ID to delete") }, "vendorId"));

            // Sales Orders
            AddTool("list_sales_orders", "List all sales orders (up to 50)");
            AddTool("search_sales_orders", "Search sales orders by order number or remarks",
                Params(new Dictionary<string, object> { ["query"] = Str("Search term") }, "query"));
            AddTool("create_sales_order", "Create a new sales order for a customer",
                Params(new Dictionary<string, object>
                {
                    ["customerId"] = Int("ID of the customer for this order"),
                    ["remarks"] = Str("Order remarks or notes")
                }, "customerId"));
            AddTool("edit_sales_order", "Edit/update an existing sales order by ID. Only provided fields will be changed.",
                Params(new Dictionary<string, object>
                {
                    ["salesOrderId"] = Int("Sales Order ID to edit"),
                    ["customerId"] = Int("New customer ID"),
                    ["remarks"] = Str("New remarks or notes"),
                    ["customerRefNumber"] = Str("New customer reference number"),
                    ["branchId"] = Int("New branch ID"),
                    ["currencyId"] = Int("New currency ID"),
                    ["salesTypeId"] = Int("New sales type ID")
                }, "salesOrderId"));
            AddTool("delete_sales_order", "Delete a sales order by ID",
                Params(new Dictionary<string, object> { ["salesOrderId"] = Int("Sales Order ID to delete") }, "salesOrderId"));

            // Purchase Orders
            AddTool("list_purchase_orders", "List all purchase orders (up to 50)");
            AddTool("search_purchase_orders", "Search purchase orders by order number or remarks",
                Params(new Dictionary<string, object> { ["query"] = Str("Search term") }, "query"));
            AddTool("create_purchase_order", "Create a new purchase order for a vendor",
                Params(new Dictionary<string, object>
                {
                    ["vendorId"] = Int("ID of the vendor for this order"),
                    ["remarks"] = Str("Order remarks or notes")
                }, "vendorId"));
            AddTool("edit_purchase_order", "Edit/update an existing purchase order by ID. Only provided fields will be changed.",
                Params(new Dictionary<string, object>
                {
                    ["purchaseOrderId"] = Int("Purchase Order ID to edit"),
                    ["vendorId"] = Int("New vendor ID"),
                    ["remarks"] = Str("New remarks or notes"),
                    ["branchId"] = Int("New branch ID"),
                    ["currencyId"] = Int("New currency ID"),
                    ["purchaseTypeId"] = Int("New purchase type ID")
                }, "purchaseOrderId"));
            AddTool("delete_purchase_order", "Delete a purchase order by ID",
                Params(new Dictionary<string, object> { ["purchaseOrderId"] = Int("Purchase Order ID to delete") }, "purchaseOrderId"));

            // Warehouses
            AddTool("list_warehouses", "List all warehouses");
            AddTool("search_warehouses", "Search warehouses by name",
                Params(new Dictionary<string, object> { ["query"] = Str("Search term") }, "query"));
            AddTool("create_warehouse", "Create a new warehouse",
                Params(new Dictionary<string, object>
                {
                    ["warehouseName"] = Str("Name of the warehouse"),
                    ["description"] = Str("Warehouse description"),
                    ["branchId"] = Int("Branch ID (optional, defaults to first branch)")
                }, "warehouseName"));
            AddTool("edit_warehouse", "Edit/update an existing warehouse by ID. Only provided fields will be changed.",
                Params(new Dictionary<string, object>
                {
                    ["warehouseId"] = Int("Warehouse ID to edit"),
                    ["warehouseName"] = Str("New name of the warehouse"),
                    ["description"] = Str("New description"),
                    ["branchId"] = Int("New branch ID")
                }, "warehouseId"));
            AddTool("delete_warehouse", "Delete a warehouse by ID",
                Params(new Dictionary<string, object> { ["warehouseId"] = Int("Warehouse ID to delete") }, "warehouseId"));

            // Branches
            AddTool("list_branches", "List all branches");
            AddTool("search_branches", "Search branches by name",
                Params(new Dictionary<string, object> { ["query"] = Str("Search term") }, "query"));
            AddTool("create_branch", "Create a new branch",
                Params(new Dictionary<string, object>
                {
                    ["branchName"] = Str("Name of the branch"),
                    ["description"] = Str("Branch description"),
                    ["address"] = Str("Street address"),
                    ["city"] = Str("City"),
                    ["state"] = Str("State"),
                    ["zipCode"] = Str("Zip code"),
                    ["phone"] = Str("Phone number"),
                    ["email"] = Str("Email address"),
                    ["contactPerson"] = Str("Contact person name"),
                    ["currencyId"] = Int("Currency ID (optional, defaults to first currency)")
                }, "branchName"));
            AddTool("edit_branch", "Edit/update an existing branch by ID. Only provided fields will be changed.",
                Params(new Dictionary<string, object>
                {
                    ["branchId"] = Int("Branch ID to edit"),
                    ["branchName"] = Str("New name of the branch"),
                    ["description"] = Str("New description"),
                    ["address"] = Str("New street address"),
                    ["city"] = Str("New city"),
                    ["state"] = Str("New state"),
                    ["zipCode"] = Str("New zip code"),
                    ["phone"] = Str("New phone number"),
                    ["email"] = Str("New email address"),
                    ["contactPerson"] = Str("New contact person name"),
                    ["currencyId"] = Int("New currency ID")
                }, "branchId"));
            AddTool("delete_branch", "Delete a branch by ID",
                Params(new Dictionary<string, object> { ["branchId"] = Int("Branch ID to delete") }, "branchId"));

            // Shipments
            AddTool("list_shipments", "List all shipments (up to 50)");
            AddTool("search_shipments", "Search shipments by shipment number",
                Params(new Dictionary<string, object> { ["query"] = Str("Search term") }, "query"));
            AddTool("create_shipment", "Create a new shipment for a sales order",
                Params(new Dictionary<string, object>
                {
                    ["salesOrderId"] = Int("ID of the sales order to ship"),
                    ["isFullShipment"] = new Dictionary<string, object> { ["type"] = "boolean", ["description"] = "Whether this is a full shipment (default true)" },
                    ["shipmentTypeId"] = Int("Shipment type ID (optional)"),
                    ["warehouseId"] = Int("Warehouse ID (optional)")
                }, "salesOrderId"));
            AddTool("edit_shipment", "Edit/update an existing shipment by ID. Only provided fields will be changed.",
                Params(new Dictionary<string, object>
                {
                    ["shipmentId"] = Int("Shipment ID to edit"),
                    ["salesOrderId"] = Int("New sales order ID"),
                    ["isFullShipment"] = new Dictionary<string, object> { ["type"] = "boolean", ["description"] = "New full shipment flag" },
                    ["shipmentTypeId"] = Int("New shipment type ID"),
                    ["warehouseId"] = Int("New warehouse ID")
                }, "shipmentId"));
            AddTool("delete_shipment", "Delete a shipment by ID",
                Params(new Dictionary<string, object> { ["shipmentId"] = Int("Shipment ID to delete") }, "shipmentId"));

            // Invoices
            AddTool("list_invoices", "List all invoices (up to 50)");
            AddTool("search_invoices", "Search invoices by invoice number",
                Params(new Dictionary<string, object> { ["query"] = Str("Search term") }, "query"));
            AddTool("create_invoice", "Create a new invoice for a shipment",
                Params(new Dictionary<string, object>
                {
                    ["shipmentId"] = Int("ID of the shipment to invoice"),
                    ["invoiceTypeId"] = Int("Invoice type ID (optional)")
                }, "shipmentId"));
            AddTool("edit_invoice", "Edit/update an existing invoice by ID. Only provided fields will be changed.",
                Params(new Dictionary<string, object>
                {
                    ["invoiceId"] = Int("Invoice ID to edit"),
                    ["shipmentId"] = Int("New shipment ID"),
                    ["invoiceTypeId"] = Int("New invoice type ID")
                }, "invoiceId"));
            AddTool("delete_invoice", "Delete an invoice by ID",
                Params(new Dictionary<string, object> { ["invoiceId"] = Int("Invoice ID to delete") }, "invoiceId"));

            // Bills
            AddTool("list_bills", "List all bills (up to 50)");
            AddTool("search_bills", "Search bills by bill number or vendor invoice number",
                Params(new Dictionary<string, object> { ["query"] = Str("Search term") }, "query"));
            AddTool("create_bill", "Create a new bill for a goods received note",
                Params(new Dictionary<string, object>
                {
                    ["goodsReceivedNoteId"] = Int("ID of the GRN to create a bill for"),
                    ["vendorDONumber"] = Str("Vendor delivery order number"),
                    ["vendorInvoiceNumber"] = Str("Vendor invoice number"),
                    ["billTypeId"] = Int("Bill type ID (optional)")
                }, "goodsReceivedNoteId"));
            AddTool("edit_bill", "Edit/update an existing bill by ID. Only provided fields will be changed.",
                Params(new Dictionary<string, object>
                {
                    ["billId"] = Int("Bill ID to edit"),
                    ["goodsReceivedNoteId"] = Int("New GRN ID"),
                    ["vendorDONumber"] = Str("New vendor delivery order number"),
                    ["vendorInvoiceNumber"] = Str("New vendor invoice number"),
                    ["billTypeId"] = Int("New bill type ID")
                }, "billId"));
            AddTool("delete_bill", "Delete a bill by ID",
                Params(new Dictionary<string, object> { ["billId"] = Int("Bill ID to delete") }, "billId"));

            // Goods Received Notes
            AddTool("list_goods_received_notes", "List all goods received notes (up to 50)");
            AddTool("search_goods_received_notes", "Search GRNs by number or vendor DO number",
                Params(new Dictionary<string, object> { ["query"] = Str("Search term") }, "query"));
            AddTool("create_goods_received_note", "Create a new goods received note for a purchase order",
                Params(new Dictionary<string, object>
                {
                    ["purchaseOrderId"] = Int("ID of the purchase order"),
                    ["vendorDONumber"] = Str("Vendor delivery order number"),
                    ["vendorInvoiceNumber"] = Str("Vendor invoice number"),
                    ["isFullReceive"] = new Dictionary<string, object> { ["type"] = "boolean", ["description"] = "Whether this is a full receive (default true)" },
                    ["warehouseId"] = Int("Warehouse ID (optional)")
                }, "purchaseOrderId"));
            AddTool("edit_goods_received_note", "Edit/update an existing goods received note by ID. Only provided fields will be changed.",
                Params(new Dictionary<string, object>
                {
                    ["goodsReceivedNoteId"] = Int("GRN ID to edit"),
                    ["purchaseOrderId"] = Int("New purchase order ID"),
                    ["vendorDONumber"] = Str("New vendor delivery order number"),
                    ["vendorInvoiceNumber"] = Str("New vendor invoice number"),
                    ["isFullReceive"] = new Dictionary<string, object> { ["type"] = "boolean", ["description"] = "New full receive flag" },
                    ["warehouseId"] = Int("New warehouse ID")
                }, "goodsReceivedNoteId"));
            AddTool("delete_goods_received_note", "Delete a goods received note by ID",
                Params(new Dictionary<string, object> { ["goodsReceivedNoteId"] = Int("GRN ID to delete") }, "goodsReceivedNoteId"));

            // Payment Vouchers
            AddTool("list_payment_vouchers", "List all payment vouchers (up to 50)");
            AddTool("search_payment_vouchers", "Search payment vouchers by number",
                Params(new Dictionary<string, object> { ["query"] = Str("Search term") }, "query"));
            AddTool("create_payment_voucher", "Create a new payment voucher for a bill",
                Params(new Dictionary<string, object>
                {
                    ["billId"] = Int("ID of the bill to pay"),
                    ["paymentAmount"] = Num("Payment amount"),
                    ["isFullPayment"] = new Dictionary<string, object> { ["type"] = "boolean", ["description"] = "Whether this is a full payment (default true)" },
                    ["paymentTypeId"] = Int("Payment type ID (optional)"),
                    ["cashBankId"] = Int("Cash/Bank ID for payment source (optional)")
                }, "billId", "paymentAmount"));
            AddTool("edit_payment_voucher", "Edit/update an existing payment voucher by ID. Only provided fields will be changed.",
                Params(new Dictionary<string, object>
                {
                    ["paymentVoucherId"] = Int("Payment Voucher ID to edit"),
                    ["billId"] = Int("New bill ID"),
                    ["paymentAmount"] = Num("New payment amount"),
                    ["isFullPayment"] = new Dictionary<string, object> { ["type"] = "boolean", ["description"] = "New full payment flag" },
                    ["paymentTypeId"] = Int("New payment type ID"),
                    ["cashBankId"] = Int("New cash/bank ID")
                }, "paymentVoucherId"));
            AddTool("delete_payment_voucher", "Delete a payment voucher by ID",
                Params(new Dictionary<string, object> { ["paymentVoucherId"] = Int("Payment Voucher ID to delete") }, "paymentVoucherId"));

            // Payment Receives
            AddTool("list_payment_receives", "List all payment receives (up to 50)");
            AddTool("search_payment_receives", "Search payment receives by number",
                Params(new Dictionary<string, object> { ["query"] = Str("Search term") }, "query"));
            AddTool("create_payment_receive", "Create a new payment receive for an invoice",
                Params(new Dictionary<string, object>
                {
                    ["invoiceId"] = Int("ID of the invoice to receive payment for"),
                    ["paymentAmount"] = Num("Payment amount"),
                    ["isFullPayment"] = new Dictionary<string, object> { ["type"] = "boolean", ["description"] = "Whether this is a full payment (default true)" },
                    ["paymentTypeId"] = Int("Payment type ID (optional)")
                }, "invoiceId", "paymentAmount"));
            AddTool("edit_payment_receive", "Edit/update an existing payment receive by ID. Only provided fields will be changed.",
                Params(new Dictionary<string, object>
                {
                    ["paymentReceiveId"] = Int("Payment Receive ID to edit"),
                    ["invoiceId"] = Int("New invoice ID"),
                    ["paymentAmount"] = Num("New payment amount"),
                    ["isFullPayment"] = new Dictionary<string, object> { ["type"] = "boolean", ["description"] = "New full payment flag" },
                    ["paymentTypeId"] = Int("New payment type ID")
                }, "paymentReceiveId"));
            AddTool("delete_payment_receive", "Delete a payment receive by ID",
                Params(new Dictionary<string, object> { ["paymentReceiveId"] = Int("Payment Receive ID to delete") }, "paymentReceiveId"));

            // Cash Banks
            AddTool("list_cash_banks", "List all cash/bank accounts");
            AddTool("search_cash_banks", "Search cash/bank accounts by name",
                Params(new Dictionary<string, object> { ["query"] = Str("Search term") }, "query"));
            AddTool("create_cash_bank", "Create a new cash/bank account",
                Params(new Dictionary<string, object>
                {
                    ["cashBankName"] = Str("Name of the cash/bank account"),
                    ["description"] = Str("Description")
                }, "cashBankName"));
            AddTool("edit_cash_bank", "Edit/update an existing cash/bank account by ID. Only provided fields will be changed.",
                Params(new Dictionary<string, object>
                {
                    ["cashBankId"] = Int("Cash Bank ID to edit"),
                    ["cashBankName"] = Str("New name of the cash/bank account"),
                    ["description"] = Str("New description")
                }, "cashBankId"));
            AddTool("delete_cash_bank", "Delete a cash/bank account by ID",
                Params(new Dictionary<string, object> { ["cashBankId"] = Int("Cash Bank ID to delete") }, "cashBankId"));

            // Currencies
            AddTool("list_currencies", "List all currencies");
            AddTool("search_currencies", "Search currencies by name or code",
                Params(new Dictionary<string, object> { ["query"] = Str("Search term") }, "query"));
            AddTool("create_currency", "Create a new currency",
                Params(new Dictionary<string, object>
                {
                    ["currencyName"] = Str("Name of the currency"),
                    ["currencyCode"] = Str("Currency code (e.g. USD, EUR)"),
                    ["description"] = Str("Description")
                }, "currencyName", "currencyCode"));
            AddTool("edit_currency", "Edit/update an existing currency by ID. Only provided fields will be changed.",
                Params(new Dictionary<string, object>
                {
                    ["currencyId"] = Int("Currency ID to edit"),
                    ["currencyName"] = Str("New name of the currency"),
                    ["currencyCode"] = Str("New currency code"),
                    ["description"] = Str("New description")
                }, "currencyId"));
            AddTool("delete_currency", "Delete a currency by ID",
                Params(new Dictionary<string, object> { ["currencyId"] = Int("Currency ID to delete") }, "currencyId"));

            // Bill Types
            AddTool("list_bill_types", "List all bill types");
            AddTool("search_bill_types", "Search bill types by name",
                Params(new Dictionary<string, object> { ["query"] = Str("Search term") }, "query"));
            AddTool("create_bill_type", "Create a new bill type",
                Params(new Dictionary<string, object>
                {
                    ["billTypeName"] = Str("Name of the bill type"),
                    ["description"] = Str("Description")
                }, "billTypeName"));
            AddTool("edit_bill_type", "Edit/update an existing bill type by ID. Only provided fields will be changed.",
                Params(new Dictionary<string, object>
                {
                    ["billTypeId"] = Int("Bill Type ID to edit"),
                    ["billTypeName"] = Str("New name of the bill type"),
                    ["description"] = Str("New description")
                }, "billTypeId"));
            AddTool("delete_bill_type", "Delete a bill type by ID",
                Params(new Dictionary<string, object> { ["billTypeId"] = Int("Bill Type ID to delete") }, "billTypeId"));

            // Customer Types
            AddTool("list_customer_types", "List all customer types");
            AddTool("search_customer_types", "Search customer types by name",
                Params(new Dictionary<string, object> { ["query"] = Str("Search term") }, "query"));
            AddTool("create_customer_type", "Create a new customer type",
                Params(new Dictionary<string, object>
                {
                    ["customerTypeName"] = Str("Name of the customer type"),
                    ["description"] = Str("Description")
                }, "customerTypeName"));
            AddTool("edit_customer_type", "Edit/update an existing customer type by ID. Only provided fields will be changed.",
                Params(new Dictionary<string, object>
                {
                    ["customerTypeId"] = Int("Customer Type ID to edit"),
                    ["customerTypeName"] = Str("New name of the customer type"),
                    ["description"] = Str("New description")
                }, "customerTypeId"));
            AddTool("delete_customer_type", "Delete a customer type by ID",
                Params(new Dictionary<string, object> { ["customerTypeId"] = Int("Customer Type ID to delete") }, "customerTypeId"));

            // Invoice Types
            AddTool("list_invoice_types", "List all invoice types");
            AddTool("search_invoice_types", "Search invoice types by name",
                Params(new Dictionary<string, object> { ["query"] = Str("Search term") }, "query"));
            AddTool("create_invoice_type", "Create a new invoice type",
                Params(new Dictionary<string, object>
                {
                    ["invoiceTypeName"] = Str("Name of the invoice type"),
                    ["description"] = Str("Description")
                }, "invoiceTypeName"));
            AddTool("edit_invoice_type", "Edit/update an existing invoice type by ID. Only provided fields will be changed.",
                Params(new Dictionary<string, object>
                {
                    ["invoiceTypeId"] = Int("Invoice Type ID to edit"),
                    ["invoiceTypeName"] = Str("New name of the invoice type"),
                    ["description"] = Str("New description")
                }, "invoiceTypeId"));
            AddTool("delete_invoice_type", "Delete an invoice type by ID",
                Params(new Dictionary<string, object> { ["invoiceTypeId"] = Int("Invoice Type ID to delete") }, "invoiceTypeId"));

            // Payment Types
            AddTool("list_payment_types", "List all payment types");
            AddTool("search_payment_types", "Search payment types by name",
                Params(new Dictionary<string, object> { ["query"] = Str("Search term") }, "query"));
            AddTool("create_payment_type", "Create a new payment type",
                Params(new Dictionary<string, object>
                {
                    ["paymentTypeName"] = Str("Name of the payment type"),
                    ["description"] = Str("Description")
                }, "paymentTypeName"));
            AddTool("edit_payment_type", "Edit/update an existing payment type by ID. Only provided fields will be changed.",
                Params(new Dictionary<string, object>
                {
                    ["paymentTypeId"] = Int("Payment Type ID to edit"),
                    ["paymentTypeName"] = Str("New name of the payment type"),
                    ["description"] = Str("New description")
                }, "paymentTypeId"));
            AddTool("delete_payment_type", "Delete a payment type by ID",
                Params(new Dictionary<string, object> { ["paymentTypeId"] = Int("Payment Type ID to delete") }, "paymentTypeId"));

            // Product Types
            AddTool("list_product_types", "List all product types");
            AddTool("search_product_types", "Search product types by name",
                Params(new Dictionary<string, object> { ["query"] = Str("Search term") }, "query"));
            AddTool("create_product_type", "Create a new product type",
                Params(new Dictionary<string, object>
                {
                    ["productTypeName"] = Str("Name of the product type"),
                    ["description"] = Str("Description")
                }, "productTypeName"));
            AddTool("edit_product_type", "Edit/update an existing product type by ID. Only provided fields will be changed.",
                Params(new Dictionary<string, object>
                {
                    ["productTypeId"] = Int("Product Type ID to edit"),
                    ["productTypeName"] = Str("New name of the product type"),
                    ["description"] = Str("New description")
                }, "productTypeId"));
            AddTool("delete_product_type", "Delete a product type by ID",
                Params(new Dictionary<string, object> { ["productTypeId"] = Int("Product Type ID to delete") }, "productTypeId"));

            // Sales Types
            AddTool("list_sales_types", "List all sales types");
            AddTool("search_sales_types", "Search sales types by name",
                Params(new Dictionary<string, object> { ["query"] = Str("Search term") }, "query"));
            AddTool("create_sales_type", "Create a new sales type",
                Params(new Dictionary<string, object>
                {
                    ["salesTypeName"] = Str("Name of the sales type"),
                    ["description"] = Str("Description")
                }, "salesTypeName"));
            AddTool("edit_sales_type", "Edit/update an existing sales type by ID. Only provided fields will be changed.",
                Params(new Dictionary<string, object>
                {
                    ["salesTypeId"] = Int("Sales Type ID to edit"),
                    ["salesTypeName"] = Str("New name of the sales type"),
                    ["description"] = Str("New description")
                }, "salesTypeId"));
            AddTool("delete_sales_type", "Delete a sales type by ID",
                Params(new Dictionary<string, object> { ["salesTypeId"] = Int("Sales Type ID to delete") }, "salesTypeId"));

            // Shipment Types
            AddTool("list_shipment_types", "List all shipment types");
            AddTool("search_shipment_types", "Search shipment types by name",
                Params(new Dictionary<string, object> { ["query"] = Str("Search term") }, "query"));
            AddTool("create_shipment_type", "Create a new shipment type",
                Params(new Dictionary<string, object>
                {
                    ["shipmentTypeName"] = Str("Name of the shipment type"),
                    ["description"] = Str("Description")
                }, "shipmentTypeName"));
            AddTool("edit_shipment_type", "Edit/update an existing shipment type by ID. Only provided fields will be changed.",
                Params(new Dictionary<string, object>
                {
                    ["shipmentTypeId"] = Int("Shipment Type ID to edit"),
                    ["shipmentTypeName"] = Str("New name of the shipment type"),
                    ["description"] = Str("New description")
                }, "shipmentTypeId"));
            AddTool("delete_shipment_type", "Delete a shipment type by ID",
                Params(new Dictionary<string, object> { ["shipmentTypeId"] = Int("Shipment Type ID to delete") }, "shipmentTypeId"));

            // Units of Measure
            AddTool("list_units_of_measure", "List all units of measure");
            AddTool("search_units_of_measure", "Search units of measure by name",
                Params(new Dictionary<string, object> { ["query"] = Str("Search term") }, "query"));
            AddTool("create_unit_of_measure", "Create a new unit of measure",
                Params(new Dictionary<string, object>
                {
                    ["unitOfMeasureName"] = Str("Name of the unit of measure"),
                    ["description"] = Str("Description")
                }, "unitOfMeasureName"));
            AddTool("edit_unit_of_measure", "Edit/update an existing unit of measure by ID. Only provided fields will be changed.",
                Params(new Dictionary<string, object>
                {
                    ["unitOfMeasureId"] = Int("Unit of Measure ID to edit"),
                    ["unitOfMeasureName"] = Str("New name of the unit of measure"),
                    ["description"] = Str("New description")
                }, "unitOfMeasureId"));
            AddTool("delete_unit_of_measure", "Delete a unit of measure by ID",
                Params(new Dictionary<string, object> { ["unitOfMeasureId"] = Int("Unit of Measure ID to delete") }, "unitOfMeasureId"));

            // Vendor Types
            AddTool("list_vendor_types", "List all vendor types");
            AddTool("search_vendor_types", "Search vendor types by name",
                Params(new Dictionary<string, object> { ["query"] = Str("Search term") }, "query"));
            AddTool("create_vendor_type", "Create a new vendor type",
                Params(new Dictionary<string, object>
                {
                    ["vendorTypeName"] = Str("Name of the vendor type"),
                    ["description"] = Str("Description")
                }, "vendorTypeName"));
            AddTool("edit_vendor_type", "Edit/update an existing vendor type by ID. Only provided fields will be changed.",
                Params(new Dictionary<string, object>
                {
                    ["vendorTypeId"] = Int("Vendor Type ID to edit"),
                    ["vendorTypeName"] = Str("New name of the vendor type"),
                    ["description"] = Str("New description")
                }, "vendorTypeId"));
            AddTool("delete_vendor_type", "Delete a vendor type by ID",
                Params(new Dictionary<string, object> { ["vendorTypeId"] = Int("Vendor Type ID to delete") }, "vendorTypeId"));

            // Purchase Types
            AddTool("list_purchase_types", "List all purchase types");
            AddTool("search_purchase_types", "Search purchase types by name",
                Params(new Dictionary<string, object> { ["query"] = Str("Search term") }, "query"));
            AddTool("create_purchase_type", "Create a new purchase type",
                Params(new Dictionary<string, object>
                {
                    ["purchaseTypeName"] = Str("Name of the purchase type"),
                    ["description"] = Str("Description")
                }, "purchaseTypeName"));
            AddTool("edit_purchase_type", "Edit/update an existing purchase type by ID. Only provided fields will be changed.",
                Params(new Dictionary<string, object>
                {
                    ["purchaseTypeId"] = Int("Purchase Type ID to edit"),
                    ["purchaseTypeName"] = Str("New name of the purchase type"),
                    ["description"] = Str("New description")
                }, "purchaseTypeId"));
            AddTool("delete_purchase_type", "Delete a purchase type by ID",
                Params(new Dictionary<string, object> { ["purchaseTypeId"] = Int("Purchase Type ID to delete") }, "purchaseTypeId"));

            // Dashboard
            AddTool("get_dashboard_data", "Get dashboard summary data including total counts of all entities and total order values");

            return tools;
        }

        #endregion
    }
}