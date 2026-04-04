using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using coderush.IntegrationTests.Infrastructure;

namespace coderush.IntegrationTests.ApiTests;

[TestClass]
public sealed class SalesOrderApiTests
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
    public async Task GetSalesOrder_ReturnsItemsAndCount()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/SalesOrder");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        Assert.IsTrue(doc.RootElement.TryGetProperty("Items", out JsonElement items));
        Assert.AreEqual(JsonValueKind.Array, items.ValueKind);
    }

    [TestMethod]
    public async Task InsertSalesOrder_AutoGeneratesOrderName()
    {
        var payload = new
        {
            value = new
            {
                BranchId = 1,
                CustomerId = 1,
                OrderDate = DateTimeOffset.UtcNow,
                DeliveryDate = DateTimeOffset.UtcNow.AddDays(7),
                CurrencyId = 1,
                SalesTypeId = 1,
                CustomerRefNumber = "REF-001",
                Remarks = "Integration test SO",
                Freight = 15.0
            }
        };

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/SalesOrder/Insert", payload);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        int soId = doc.RootElement.GetProperty("SalesOrderId").GetInt32();
        string? soName = doc.RootElement.GetProperty("SalesOrderName").GetString();

        Assert.IsTrue(soId > 0);
        Assert.IsTrue(soName?.StartsWith("SO") == true,
            $"Expected SalesOrderName to start with 'SO', got '{soName}'.");
    }

    [TestMethod]
    public async Task GetNotShippedYet_ReturnsSalesOrders()
    {
        // Insert an SO (no Shipment linked).
        var payload = new
        {
            value = new
            {
                BranchId = 1,
                CustomerId = 1,
                OrderDate = DateTimeOffset.UtcNow,
                DeliveryDate = DateTimeOffset.UtcNow.AddDays(5),
                CurrencyId = 1,
                SalesTypeId = 1
            }
        };
        await _client.PostAsJsonAsync("/api/SalesOrder/Insert", payload);

        HttpResponseMessage response = await _client.GetAsync("/api/SalesOrder/GetNotShippedYet");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        Assert.AreEqual(JsonValueKind.Array, doc.RootElement.ValueKind);
    }

    [TestMethod]
    public async Task GetById_ExistingId_ReturnsSalesOrderWithLines()
    {
        var payload = new
        {
            value = new
            {
                BranchId = 1,
                CustomerId = 1,
                OrderDate = DateTimeOffset.UtcNow,
                DeliveryDate = DateTimeOffset.UtcNow.AddDays(3),
                CurrencyId = 1,
                SalesTypeId = 1
            }
        };
        HttpResponseMessage insertResponse = await _client.PostAsJsonAsync("/api/SalesOrder/Insert", payload);
        JsonDocument insertDoc = await JsonDocument.ParseAsync(await insertResponse.Content.ReadAsStreamAsync());
        int soId = insertDoc.RootElement.GetProperty("SalesOrderId").GetInt32();

        HttpResponseMessage response = await _client.GetAsync($"/api/SalesOrder/GetById/{soId}");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        Assert.AreEqual(soId, doc.RootElement.GetProperty("SalesOrderId").GetInt32());
        Assert.IsTrue(doc.RootElement.TryGetProperty("SalesOrderLines", out _));
    }

    [TestMethod]
    public async Task UpdateSalesOrder_ValidPayload_Succeeds()
    {
        var insertPayload = new
        {
            value = new
            {
                BranchId = 1,
                CustomerId = 1,
                OrderDate = DateTimeOffset.UtcNow,
                DeliveryDate = DateTimeOffset.UtcNow.AddDays(7),
                CurrencyId = 1,
                SalesTypeId = 1,
                Freight = 5.0
            }
        };
        HttpResponseMessage insertResponse = await _client.PostAsJsonAsync("/api/SalesOrder/Insert", insertPayload);
        JsonDocument insertDoc = await JsonDocument.ParseAsync(await insertResponse.Content.ReadAsStreamAsync());
        int soId = insertDoc.RootElement.GetProperty("SalesOrderId").GetInt32();

        var updatePayload = new
        {
            value = new
            {
                SalesOrderId = soId,
                BranchId = 1,
                CustomerId = 1,
                OrderDate = DateTimeOffset.UtcNow,
                DeliveryDate = DateTimeOffset.UtcNow.AddDays(14),
                CurrencyId = 1,
                SalesTypeId = 1,
                Remarks = "Updated SO",
                Freight = 25.0
            }
        };

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/SalesOrder/Update", updatePayload);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task RemoveSalesOrder_ValidKey_Succeeds()
    {
        var insertPayload = new
        {
            value = new
            {
                BranchId = 1,
                CustomerId = 1,
                OrderDate = DateTimeOffset.UtcNow,
                DeliveryDate = DateTimeOffset.UtcNow.AddDays(1),
                CurrencyId = 1,
                SalesTypeId = 1
            }
        };
        HttpResponseMessage insertResponse = await _client.PostAsJsonAsync("/api/SalesOrder/Insert", insertPayload);
        JsonDocument insertDoc = await JsonDocument.ParseAsync(await insertResponse.Content.ReadAsStreamAsync());
        int soId = insertDoc.RootElement.GetProperty("SalesOrderId").GetInt32();

        var removePayload = new { key = soId.ToString() };
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/SalesOrder/Remove", removePayload);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }
}
