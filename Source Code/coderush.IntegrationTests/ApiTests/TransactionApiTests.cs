using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using coderush.IntegrationTests.Infrastructure;

namespace coderush.IntegrationTests.ApiTests;

/// <summary>
/// Integration tests for transactional / downstream API controllers:
/// GoodsReceivedNote, Shipment, Bill, Invoice, PaymentVoucher, PaymentReceive.
/// These depend on PurchaseOrder / SalesOrder existing, so each test
/// creates its own prerequisite data.
/// </summary>
[TestClass]
public sealed class TransactionApiTests
{
    private static CustomWebApplicationFactory _factory = null!;
    private static HttpClient _client = null!;

    [ClassInitialize]
    public static async Task ClassInit(TestContext _)
    {
        _factory = new CustomWebApplicationFactory();
        _client = await AuthHelper.CreateAuthenticatedClientAsync(_factory);
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }

    // ────────── GoodsReceivedNote ──────────

    [TestMethod]
    public async Task GoodsReceivedNote_GetAll_ReturnsItemsAndCount()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/GoodsReceivedNote");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        await AssertItemsAndCount(response);
    }

    [TestMethod]
    public async Task GoodsReceivedNote_Insert_AutoGeneratesName()
    {
        int poId = await CreatePurchaseOrder();

        var payload = new
        {
            value = new
            {
                PurchaseOrderId = poId,
                GRNDate = DateTimeOffset.UtcNow,
                VendorDONumber = "VDO-001",
                VendorInvoiceNumber = "VINV-001",
                WarehouseId = 1,
                IsFullReceive = true
            }
        };

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/GoodsReceivedNote/Insert", payload);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        string? grnName = doc.RootElement.GetProperty("GoodsReceivedNoteName").GetString();
        Assert.IsTrue(grnName?.StartsWith("GRN") == true);
    }

    [TestMethod]
    public async Task GoodsReceivedNote_GetNotBilledYet_ReturnsArray()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/GoodsReceivedNote/GetNotBilledYet");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        Assert.AreEqual(JsonValueKind.Array, doc.RootElement.ValueKind);
    }

    // ────────── Shipment ──────────

    [TestMethod]
    public async Task Shipment_GetAll_ReturnsItemsAndCount()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/Shipment");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        await AssertItemsAndCount(response);
    }

    [TestMethod]
    public async Task Shipment_Insert_AutoGeneratesName()
    {
        int soId = await CreateSalesOrder();

        var payload = new
        {
            value = new
            {
                SalesOrderId = soId,
                ShipmentDate = DateTimeOffset.UtcNow,
                ShipmentTypeId = 1,
                WarehouseId = 1,
                IsFullShipment = true
            }
        };

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/Shipment/Insert", payload);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        string? name = doc.RootElement.GetProperty("ShipmentName").GetString();
        Assert.IsTrue(name?.StartsWith("DO") == true);
    }

    [TestMethod]
    public async Task Shipment_GetNotInvoicedYet_ReturnsArray()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/Shipment/GetNotInvoicedYet");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        Assert.AreEqual(JsonValueKind.Array, doc.RootElement.ValueKind);
    }

    // ────────── Bill ──────────

    [TestMethod]
    public async Task Bill_GetAll_ReturnsItemsAndCount()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/Bill");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        await AssertItemsAndCount(response);
    }

    [TestMethod]
    public async Task Bill_Insert_AutoGeneratesName()
    {
        int grnId = await CreateGoodsReceivedNote();

        var payload = new
        {
            value = new
            {
                GoodsReceivedNoteId = grnId,
                VendorDONumber = "VDO-BILL",
                VendorInvoiceNumber = "VINV-BILL",
                BillDate = DateTimeOffset.UtcNow,
                BillDueDate = DateTimeOffset.UtcNow.AddDays(30),
                BillTypeId = 1
            }
        };

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/Bill/Insert", payload);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        string? name = doc.RootElement.GetProperty("BillName").GetString();
        Assert.IsTrue(name?.StartsWith("BILL") == true);
    }

    [TestMethod]
    public async Task Bill_GetNotPaidYet_ReturnsArray()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/Bill/GetNotPaidYet");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        Assert.AreEqual(JsonValueKind.Array, doc.RootElement.ValueKind);
    }

    // ────────── Invoice ──────────

    [TestMethod]
    public async Task Invoice_GetAll_ReturnsItemsAndCount()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/Invoice");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        await AssertItemsAndCount(response);
    }

    [TestMethod]
    public async Task Invoice_Insert_AutoGeneratesName()
    {
        int shipmentId = await CreateShipment();

        var payload = new
        {
            value = new
            {
                ShipmentId = shipmentId,
                InvoiceDate = DateTimeOffset.UtcNow,
                InvoiceDueDate = DateTimeOffset.UtcNow.AddDays(30),
                InvoiceTypeId = 1
            }
        };

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/Invoice/Insert", payload);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        string? name = doc.RootElement.GetProperty("InvoiceName").GetString();
        Assert.IsTrue(name?.StartsWith("INV") == true);
    }

    [TestMethod]
    public async Task Invoice_GetNotPaidYet_ReturnsArray()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/Invoice/GetNotPaidYet");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        Assert.AreEqual(JsonValueKind.Array, doc.RootElement.ValueKind);
    }

    // ────────── PaymentVoucher ──────────

    [TestMethod]
    public async Task PaymentVoucher_GetAll_ReturnsItemsAndCount()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/PaymentVoucher");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        await AssertItemsAndCount(response);
    }

    [TestMethod]
    public async Task PaymentVoucher_Insert_AutoGeneratesName()
    {
        int billId = await CreateBill();

        var payload = new
        {
            value = new
            {
                BillId = billId,
                PaymentDate = DateTimeOffset.UtcNow,
                PaymentTypeId = 1,
                PaymentAmount = 500.0,
                CashBankId = 1,
                IsFullPayment = true
            }
        };

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/PaymentVoucher/Insert", payload);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        string? name = doc.RootElement.GetProperty("PaymentVoucherName").GetString();
        Assert.IsTrue(name?.StartsWith("PAYVCH") == true);
    }

    // ────────── PaymentReceive ──────────

    [TestMethod]
    public async Task PaymentReceive_GetAll_ReturnsItemsAndCount()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/PaymentReceive");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        await AssertItemsAndCount(response);
    }

    [TestMethod]
    public async Task PaymentReceive_Insert_AutoGeneratesName()
    {
        int invoiceId = await CreateInvoice();

        var payload = new
        {
            value = new
            {
                InvoiceId = invoiceId,
                PaymentDate = DateTimeOffset.UtcNow,
                PaymentTypeId = 1,
                PaymentAmount = 750.0,
                IsFullPayment = true
            }
        };

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/PaymentReceive/Insert", payload);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        string? name = doc.RootElement.GetProperty("PaymentReceiveName").GetString();
        Assert.IsTrue(name?.StartsWith("PAYRCV") == true);
    }

    // ────────── Helpers: create prerequisite entities ──────────

    private static async Task<int> CreatePurchaseOrder()
    {
        var payload = new
        {
            value = new
            {
                BranchId = 1, VendorId = 1,
                OrderDate = DateTimeOffset.UtcNow,
                DeliveryDate = DateTimeOffset.UtcNow.AddDays(7),
                CurrencyId = 1, PurchaseTypeId = 1
            }
        };
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/PurchaseOrder/Insert", payload);
        JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        return doc.RootElement.GetProperty("PurchaseOrderId").GetInt32();
    }

    private static async Task<int> CreateSalesOrder()
    {
        var payload = new
        {
            value = new
            {
                BranchId = 1, CustomerId = 1,
                OrderDate = DateTimeOffset.UtcNow,
                DeliveryDate = DateTimeOffset.UtcNow.AddDays(7),
                CurrencyId = 1, SalesTypeId = 1
            }
        };
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/SalesOrder/Insert", payload);
        JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        return doc.RootElement.GetProperty("SalesOrderId").GetInt32();
    }

    private static async Task<int> CreateGoodsReceivedNote()
    {
        int poId = await CreatePurchaseOrder();
        var payload = new
        {
            value = new
            {
                PurchaseOrderId = poId,
                GRNDate = DateTimeOffset.UtcNow,
                VendorDONumber = "VDO-T",
                VendorInvoiceNumber = "VINV-T",
                WarehouseId = 1,
                IsFullReceive = true
            }
        };
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/GoodsReceivedNote/Insert", payload);
        JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        return doc.RootElement.GetProperty("GoodsReceivedNoteId").GetInt32();
    }

    private static async Task<int> CreateShipment()
    {
        int soId = await CreateSalesOrder();
        var payload = new
        {
            value = new
            {
                SalesOrderId = soId,
                ShipmentDate = DateTimeOffset.UtcNow,
                ShipmentTypeId = 1,
                WarehouseId = 1,
                IsFullShipment = true
            }
        };
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/Shipment/Insert", payload);
        JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        return doc.RootElement.GetProperty("ShipmentId").GetInt32();
    }

    private static async Task<int> CreateBill()
    {
        int grnId = await CreateGoodsReceivedNote();
        var payload = new
        {
            value = new
            {
                GoodsReceivedNoteId = grnId,
                VendorDONumber = "VDO-B",
                VendorInvoiceNumber = "VINV-B",
                BillDate = DateTimeOffset.UtcNow,
                BillDueDate = DateTimeOffset.UtcNow.AddDays(30),
                BillTypeId = 1
            }
        };
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/Bill/Insert", payload);
        JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        return doc.RootElement.GetProperty("BillId").GetInt32();
    }

    private static async Task<int> CreateInvoice()
    {
        int shipmentId = await CreateShipment();
        var payload = new
        {
            value = new
            {
                ShipmentId = shipmentId,
                InvoiceDate = DateTimeOffset.UtcNow,
                InvoiceDueDate = DateTimeOffset.UtcNow.AddDays(30),
                InvoiceTypeId = 1
            }
        };
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/Invoice/Insert", payload);
        JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        return doc.RootElement.GetProperty("InvoiceId").GetInt32();
    }

    private static async Task AssertItemsAndCount(HttpResponseMessage response)
    {
        JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        Assert.IsTrue(doc.RootElement.TryGetProperty("Items", out JsonElement items));
        Assert.AreEqual(JsonValueKind.Array, items.ValueKind);
        Assert.IsTrue(doc.RootElement.TryGetProperty("Count", out JsonElement count));
        Assert.AreEqual(count.GetInt32(), items.GetArrayLength());
    }
}
