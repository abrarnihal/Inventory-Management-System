using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using coderush.IntegrationTests.Infrastructure;

namespace coderush.IntegrationTests.ApiTests;

[TestClass]
public sealed class ProductApiTests
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
    public async Task GetProduct_ReturnsItemsAndCount()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/Product");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        Assert.IsTrue(doc.RootElement.TryGetProperty("Items", out JsonElement items));
        Assert.AreEqual(JsonValueKind.Array, items.ValueKind);
        Assert.IsTrue(doc.RootElement.GetProperty("Count").GetInt32() >= 0);
    }

    [TestMethod]
    public async Task InsertProduct_ValidPayload_ReturnsCreatedProduct()
    {
        var payload = new
        {
            value = new
            {
                ProductName = "Integration Test Product",
                ProductCode = "ITP-001",
                Barcode = "1234567890",
                Description = "Test product for integration tests",
                UnitOfMeasureId = 1,
                BranchId = 1,
                CurrencyId = 1,
                DefaultBuyingPrice = 25.50,
                DefaultSellingPrice = 49.99
            }
        };

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/Product/Insert", payload);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        Assert.IsTrue(doc.RootElement.GetProperty("ProductId").GetInt32() > 0);
        Assert.AreEqual("Integration Test Product",
            doc.RootElement.GetProperty("ProductName").GetString());
    }

    [TestMethod]
    public async Task UpdateProduct_ValidPayload_ReturnsUpdatedProduct()
    {
        var insertPayload = new
        {
            value = new
            {
                ProductName = "Product Before",
                UnitOfMeasureId = 1,
                BranchId = 1,
                CurrencyId = 1,
                DefaultBuyingPrice = 10.0,
                DefaultSellingPrice = 20.0
            }
        };
        HttpResponseMessage insertResponse = await _client.PostAsJsonAsync("/api/Product/Insert", insertPayload);
        JsonDocument insertDoc = await JsonDocument.ParseAsync(await insertResponse.Content.ReadAsStreamAsync());
        int productId = insertDoc.RootElement.GetProperty("ProductId").GetInt32();

        var updatePayload = new
        {
            value = new
            {
                ProductId = productId,
                ProductName = "Product After",
                UnitOfMeasureId = 1,
                BranchId = 1,
                CurrencyId = 1,
                DefaultBuyingPrice = 15.0,
                DefaultSellingPrice = 30.0
            }
        };

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/Product/Update", updatePayload);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        Assert.AreEqual("Product After", doc.RootElement.GetProperty("ProductName").GetString());
    }

    [TestMethod]
    public async Task RemoveProduct_ValidKey_ReturnsRemovedProduct()
    {
        var insertPayload = new
        {
            value = new
            {
                ProductName = "Product To Remove",
                UnitOfMeasureId = 1,
                BranchId = 1,
                CurrencyId = 1
            }
        };
        HttpResponseMessage insertResponse = await _client.PostAsJsonAsync("/api/Product/Insert", insertPayload);
        JsonDocument insertDoc = await JsonDocument.ParseAsync(await insertResponse.Content.ReadAsStreamAsync());
        int productId = insertDoc.RootElement.GetProperty("ProductId").GetInt32();

        var removePayload = new { key = productId.ToString() };
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/Product/Remove", removePayload);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }
}
