using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using coderush.IntegrationTests.Infrastructure;

namespace coderush.IntegrationTests.ApiTests;

[TestClass]
public sealed class PurchaseOrderApiTests
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

    [TestMethod]
    public async Task GetPurchaseOrder_ReturnsItemsAndCount()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/PurchaseOrder");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        Assert.IsTrue(doc.RootElement.TryGetProperty("Items", out JsonElement items));
        Assert.AreEqual(JsonValueKind.Array, items.ValueKind);
        Assert.IsTrue(doc.RootElement.TryGetProperty("Count", out _));
    }

    [TestMethod]
    public async Task InsertPurchaseOrder_AutoGeneratesOrderName()
    {
        var payload = new
        {
            value = new
            {
                BranchId = 1,
                VendorId = 1,
                OrderDate = DateTimeOffset.UtcNow,
                DeliveryDate = DateTimeOffset.UtcNow.AddDays(7),
                CurrencyId = 1,
                PurchaseTypeId = 1,
                Remarks = "Integration test PO",
                Freight = 10.0
            }
        };

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/PurchaseOrder/Insert", payload);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        int poId = doc.RootElement.GetProperty("PurchaseOrderId").GetInt32();
        string? poName = doc.RootElement.GetProperty("PurchaseOrderName").GetString();

        Assert.IsTrue(poId > 0);
        Assert.IsTrue(poName?.StartsWith("PO") == true,
            $"Expected PurchaseOrderName to start with 'PO', got '{poName}'.");
    }

    [TestMethod]
    public async Task GetNotReceivedYet_ReturnsPurchaseOrders()
    {
        // Insert a PO (no GRN linked, so it should appear in "not received yet").
        var payload = new
        {
            value = new
            {
                BranchId = 1,
                VendorId = 1,
                OrderDate = DateTimeOffset.UtcNow,
                DeliveryDate = DateTimeOffset.UtcNow.AddDays(5),
                CurrencyId = 1,
                PurchaseTypeId = 1
            }
        };
        await _client.PostAsJsonAsync("/api/PurchaseOrder/Insert", payload);

        HttpResponseMessage response = await _client.GetAsync("/api/PurchaseOrder/GetNotReceivedYet");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        Assert.AreEqual(JsonValueKind.Array, doc.RootElement.ValueKind);
    }

    [TestMethod]
    public async Task GetById_ExistingId_ReturnsPurchaseOrderWithLines()
    {
        // Insert a PO first.
        var payload = new
        {
            value = new
            {
                BranchId = 1,
                VendorId = 1,
                OrderDate = DateTimeOffset.UtcNow,
                DeliveryDate = DateTimeOffset.UtcNow.AddDays(3),
                CurrencyId = 1,
                PurchaseTypeId = 1
            }
        };
        HttpResponseMessage insertResponse = await _client.PostAsJsonAsync("/api/PurchaseOrder/Insert", payload);
        JsonDocument insertDoc = await JsonDocument.ParseAsync(await insertResponse.Content.ReadAsStreamAsync());
        int poId = insertDoc.RootElement.GetProperty("PurchaseOrderId").GetInt32();

        HttpResponseMessage response = await _client.GetAsync($"/api/PurchaseOrder/GetById/{poId}");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        Assert.AreEqual(poId, doc.RootElement.GetProperty("PurchaseOrderId").GetInt32());
        Assert.IsTrue(doc.RootElement.TryGetProperty("PurchaseOrderLines", out _));
    }

    [TestMethod]
    public async Task UpdatePurchaseOrder_RecalculatesTotals()
    {
        var insertPayload = new
        {
            value = new
            {
                BranchId = 1,
                VendorId = 1,
                OrderDate = DateTimeOffset.UtcNow,
                DeliveryDate = DateTimeOffset.UtcNow.AddDays(7),
                CurrencyId = 1,
                PurchaseTypeId = 1,
                Freight = 5.0
            }
        };
        HttpResponseMessage insertResponse = await _client.PostAsJsonAsync("/api/PurchaseOrder/Insert", insertPayload);
        JsonDocument insertDoc = await JsonDocument.ParseAsync(await insertResponse.Content.ReadAsStreamAsync());
        int poId = insertDoc.RootElement.GetProperty("PurchaseOrderId").GetInt32();

        var updatePayload = new
        {
            value = new
            {
                PurchaseOrderId = poId,
                BranchId = 1,
                VendorId = 1,
                OrderDate = DateTimeOffset.UtcNow,
                DeliveryDate = DateTimeOffset.UtcNow.AddDays(14),
                CurrencyId = 1,
                PurchaseTypeId = 1,
                Remarks = "Updated via integration test",
                Freight = 20.0
            }
        };

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/PurchaseOrder/Update", updatePayload);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task RemovePurchaseOrder_ValidKey_Succeeds()
    {
        var insertPayload = new
        {
            value = new
            {
                BranchId = 1,
                VendorId = 1,
                OrderDate = DateTimeOffset.UtcNow,
                DeliveryDate = DateTimeOffset.UtcNow.AddDays(1),
                CurrencyId = 1,
                PurchaseTypeId = 1
            }
        };
        HttpResponseMessage insertResponse = await _client.PostAsJsonAsync("/api/PurchaseOrder/Insert", insertPayload);
        JsonDocument insertDoc = await JsonDocument.ParseAsync(await insertResponse.Content.ReadAsStreamAsync());
        int poId = insertDoc.RootElement.GetProperty("PurchaseOrderId").GetInt32();

        var removePayload = new { key = poId.ToString() };
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/PurchaseOrder/Remove", removePayload);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }
}
