using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using coderush.IntegrationTests.Infrastructure;

namespace coderush.IntegrationTests.ApiTests;

/// <summary>
/// Integration tests for PurchaseOrderLine and SalesOrderLine API controllers.
/// These controllers use custom request headers (PurchaseOrderId / SalesOrderId)
/// for GET filtering, and automatically recalculate line totals on Insert/Update.
/// </summary>
[TestClass]
public sealed class OrderLineApiTests
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

    // ────────── PurchaseOrderLine ──────────

    [TestMethod]
    public async Task PurchaseOrderLine_GetByHeader_ReturnsItemsAndCount()
    {
        int poId = await CreatePurchaseOrder();

        // The controller reads PurchaseOrderId from a request header.
        HttpRequestMessage request = new(HttpMethod.Get, "/api/PurchaseOrderLine");
        request.Headers.Add("PurchaseOrderId", poId.ToString());

        HttpResponseMessage response = await _client.SendAsync(request);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        Assert.IsTrue(doc.RootElement.TryGetProperty("Items", out _));
        Assert.IsTrue(doc.RootElement.TryGetProperty("Count", out _));
    }

    [TestMethod]
    public async Task PurchaseOrderLine_Insert_RecalculatesTotals()
    {
        int poId = await CreatePurchaseOrder();

        var payload = new
        {
            value = new
            {
                PurchaseOrderId = poId,
                ProductId = 1,
                Description = "Test PO Line",
                Quantity = 10.0,
                Price = 25.0,
                DiscountPercentage = 5.0,
                TaxPercentage = 10.0
            }
        };

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/PurchaseOrderLine/Insert", payload);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());

        // Amount = Quantity * Price = 10 * 25 = 250
        double amount = doc.RootElement.GetProperty("Amount").GetDouble();
        Assert.AreEqual(250.0, amount, 0.01);

        // DiscountAmount = (5% * 250) = 12.50
        double discount = doc.RootElement.GetProperty("DiscountAmount").GetDouble();
        Assert.AreEqual(12.50, discount, 0.01);

        // SubTotal = 250 - 12.50 = 237.50
        double subTotal = doc.RootElement.GetProperty("SubTotal").GetDouble();
        Assert.AreEqual(237.50, subTotal, 0.01);

        // TaxAmount = (10% * 237.50) = 23.75
        double tax = doc.RootElement.GetProperty("TaxAmount").GetDouble();
        Assert.AreEqual(23.75, tax, 0.01);

        // Total = 237.50 + 23.75 = 261.25
        double total = doc.RootElement.GetProperty("Total").GetDouble();
        Assert.AreEqual(261.25, total, 0.01);
    }

    [TestMethod]
    public async Task PurchaseOrderLine_InsertAndRemove_Succeeds()
    {
        int poId = await CreatePurchaseOrder();
        int lineId = await InsertPurchaseOrderLine(poId);

        var removePayload = new { key = lineId.ToString() };
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/PurchaseOrderLine/Remove", removePayload);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    // ────────── SalesOrderLine ──────────

    [TestMethod]
    public async Task SalesOrderLine_GetByHeader_ReturnsItemsAndCount()
    {
        int soId = await CreateSalesOrder();

        HttpRequestMessage request = new(HttpMethod.Get, "/api/SalesOrderLine");
        request.Headers.Add("SalesOrderId", soId.ToString());

        HttpResponseMessage response = await _client.SendAsync(request);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        Assert.IsTrue(doc.RootElement.TryGetProperty("Items", out _));
        Assert.IsTrue(doc.RootElement.TryGetProperty("Count", out _));
    }

    [TestMethod]
    public async Task SalesOrderLine_Insert_RecalculatesTotals()
    {
        int soId = await CreateSalesOrder();

        var payload = new
        {
            value = new
            {
                SalesOrderId = soId,
                ProductId = 1,
                Description = "Test SO Line",
                Quantity = 5.0,
                Price = 100.0,
                DiscountPercentage = 10.0,
                TaxPercentage = 8.0
            }
        };

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/SalesOrderLine/Insert", payload);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());

        // Amount = 5 * 100 = 500
        Assert.AreEqual(500.0, doc.RootElement.GetProperty("Amount").GetDouble(), 0.01);
        // DiscountAmount = 10% * 500 = 50
        Assert.AreEqual(50.0, doc.RootElement.GetProperty("DiscountAmount").GetDouble(), 0.01);
        // SubTotal = 500 - 50 = 450
        Assert.AreEqual(450.0, doc.RootElement.GetProperty("SubTotal").GetDouble(), 0.01);
        // TaxAmount = 8% * 450 = 36
        Assert.AreEqual(36.0, doc.RootElement.GetProperty("TaxAmount").GetDouble(), 0.01);
        // Total = 450 + 36 = 486
        Assert.AreEqual(486.0, doc.RootElement.GetProperty("Total").GetDouble(), 0.01);
    }

    [TestMethod]
    public async Task SalesOrderLine_GetByShipmentIdHeader_ReturnsItems()
    {
        // Create SO → Shipment, then query by ShipmentId header.
        int soId = await CreateSalesOrder();
        int shipmentId = await CreateShipment(soId);

        HttpRequestMessage request = new(HttpMethod.Get, "/api/SalesOrderLine/GetSalesOrderLineByShipmentId");
        request.Headers.Add("ShipmentId", shipmentId.ToString());

        HttpResponseMessage response = await _client.SendAsync(request);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        Assert.IsTrue(doc.RootElement.TryGetProperty("Items", out _));
    }

    [TestMethod]
    public async Task SalesOrderLine_InsertAndRemove_Succeeds()
    {
        int soId = await CreateSalesOrder();
        int lineId = await InsertSalesOrderLine(soId);

        var removePayload = new { key = lineId.ToString() };
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/SalesOrderLine/Remove", removePayload);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    // ────────── Helpers ──────────

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
        HttpResponseMessage r = await _client.PostAsJsonAsync("/api/PurchaseOrder/Insert", payload);
        JsonDocument d = await JsonDocument.ParseAsync(await r.Content.ReadAsStreamAsync());
        return d.RootElement.GetProperty("PurchaseOrderId").GetInt32();
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
        HttpResponseMessage r = await _client.PostAsJsonAsync("/api/SalesOrder/Insert", payload);
        JsonDocument d = await JsonDocument.ParseAsync(await r.Content.ReadAsStreamAsync());
        return d.RootElement.GetProperty("SalesOrderId").GetInt32();
    }

    private static async Task<int> CreateShipment(int salesOrderId)
    {
        var payload = new
        {
            value = new
            {
                SalesOrderId = salesOrderId,
                ShipmentDate = DateTimeOffset.UtcNow,
                ShipmentTypeId = 1, WarehouseId = 1,
                IsFullShipment = true
            }
        };
        HttpResponseMessage r = await _client.PostAsJsonAsync("/api/Shipment/Insert", payload);
        JsonDocument d = await JsonDocument.ParseAsync(await r.Content.ReadAsStreamAsync());
        return d.RootElement.GetProperty("ShipmentId").GetInt32();
    }

    private static async Task<int> InsertPurchaseOrderLine(int poId)
    {
        var payload = new
        {
            value = new
            {
                PurchaseOrderId = poId, ProductId = 1,
                Description = "Line item", Quantity = 1.0, Price = 10.0,
                DiscountPercentage = 0.0, TaxPercentage = 0.0
            }
        };
        HttpResponseMessage r = await _client.PostAsJsonAsync("/api/PurchaseOrderLine/Insert", payload);
        JsonDocument d = await JsonDocument.ParseAsync(await r.Content.ReadAsStreamAsync());
        return d.RootElement.GetProperty("PurchaseOrderLineId").GetInt32();
    }

    private static async Task<int> InsertSalesOrderLine(int soId)
    {
        var payload = new
        {
            value = new
            {
                SalesOrderId = soId, ProductId = 1,
                Description = "Line item", Quantity = 1.0, Price = 10.0,
                DiscountPercentage = 0.0, TaxPercentage = 0.0
            }
        };
        HttpResponseMessage r = await _client.PostAsJsonAsync("/api/SalesOrderLine/Insert", payload);
        JsonDocument d = await JsonDocument.ParseAsync(await r.Content.ReadAsStreamAsync());
        return d.RootElement.GetProperty("SalesOrderLineId").GetInt32();
    }
}
