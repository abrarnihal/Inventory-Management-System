using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using coderush.IntegrationTests.Infrastructure;

namespace coderush.IntegrationTests.ApiTests;

/// <summary>
/// Integration tests for all reference-data / lookup-table API controllers:
/// Branch, Currency, UnitOfMeasure, Warehouse, CashBank,
/// CustomerType, VendorType, ProductType, BillType, InvoiceType,
/// PaymentType, PurchaseType, SalesType, ShipmentType.
/// Each follows the same GET / Insert / Update / Remove pattern.
/// </summary>
[TestClass]
public sealed class ReferenceDataApiTests
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

    // ───────────────────────── Branch ─────────────────────────

    [TestMethod]
    public async Task Branch_GetAll_ReturnsItemsAndCount()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/Branch");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        await AssertItemsAndCount(response);
    }

    [TestMethod]
    public async Task Branch_InsertAndRemove_Succeeds()
    {
        int id = await InsertEntity("/api/Branch/Insert",
            new { value = new { BranchName = "Test Branch", Description = "Int test", CurrencyId = 1 } },
            "BranchId");
        Assert.IsTrue(id > 0);

        await RemoveEntity("/api/Branch/Remove", id);
    }

    // ───────────────────────── Currency ─────────────────────────

    [TestMethod]
    public async Task Currency_GetAll_ReturnsItemsAndCount()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/Currency");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        await AssertItemsAndCount(response);
    }

    [TestMethod]
    public async Task Currency_InsertAndRemove_Succeeds()
    {
        int id = await InsertEntity("/api/Currency/Insert",
            new { value = new { CurrencyName = "Test Dollar", CurrencyCode = "TSD", Description = "Int test" } },
            "CurrencyId");
        Assert.IsTrue(id > 0);

        await RemoveEntity("/api/Currency/Remove", id);
    }

    // ───────────────────────── UnitOfMeasure ─────────────────────────

    [TestMethod]
    public async Task UnitOfMeasure_GetAll_ReturnsItemsAndCount()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/UnitOfMeasure");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        await AssertItemsAndCount(response);
    }

    [TestMethod]
    public async Task UnitOfMeasure_InsertAndRemove_Succeeds()
    {
        int id = await InsertEntity("/api/UnitOfMeasure/Insert",
            new { value = new { UnitOfMeasureName = "BOX", Description = "Int test" } },
            "UnitOfMeasureId");
        Assert.IsTrue(id > 0);

        await RemoveEntity("/api/UnitOfMeasure/Remove", id);
    }

    // ───────────────────────── Warehouse ─────────────────────────

    [TestMethod]
    public async Task Warehouse_GetAll_ReturnsItemsAndCount()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/Warehouse");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        await AssertItemsAndCount(response);
    }

    [TestMethod]
    public async Task Warehouse_InsertAndRemove_Succeeds()
    {
        int id = await InsertEntity("/api/Warehouse/Insert",
            new { value = new { WarehouseName = "Test Warehouse", Description = "Int test", BranchId = 1 } },
            "WarehouseId");
        Assert.IsTrue(id > 0);

        await RemoveEntity("/api/Warehouse/Remove", id);
    }

    // ───────────────────────── CashBank ─────────────────────────

    [TestMethod]
    public async Task CashBank_GetAll_ReturnsItemsAndCount()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/CashBank");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        await AssertItemsAndCount(response);
    }

    [TestMethod]
    public async Task CashBank_InsertAndRemove_Succeeds()
    {
        int id = await InsertEntity("/api/CashBank/Insert",
            new { value = new { CashBankName = "Test CashBank", Description = "Int test" } },
            "CashBankId");
        Assert.IsTrue(id > 0);

        await RemoveEntity("/api/CashBank/Remove", id);
    }

    // ───────────────────────── CustomerType ─────────────────────────

    [TestMethod]
    public async Task CustomerType_GetAll_ReturnsItemsAndCount()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/CustomerType");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        await AssertItemsAndCount(response);
    }

    [TestMethod]
    public async Task CustomerType_InsertAndRemove_Succeeds()
    {
        int id = await InsertEntity("/api/CustomerType/Insert",
            new { value = new { CustomerTypeName = "Test CustType", Description = "Int test" } },
            "CustomerTypeId");
        Assert.IsTrue(id > 0);

        await RemoveEntity("/api/CustomerType/Remove", id);
    }

    // ───────────────────────── VendorType ─────────────────────────

    [TestMethod]
    public async Task VendorType_GetAll_ReturnsItemsAndCount()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/VendorType");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        await AssertItemsAndCount(response);
    }

    [TestMethod]
    public async Task VendorType_InsertAndRemove_Succeeds()
    {
        int id = await InsertEntity("/api/VendorType/Insert",
            new { value = new { VendorTypeName = "Test VendType", Description = "Int test" } },
            "VendorTypeId");
        Assert.IsTrue(id > 0);

        await RemoveEntity("/api/VendorType/Remove", id);
    }

    // ───────────────────────── ProductType ─────────────────────────

    [TestMethod]
    public async Task ProductType_GetAll_ReturnsItemsAndCount()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/ProductType");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        await AssertItemsAndCount(response);
    }

    [TestMethod]
    public async Task ProductType_InsertAndRemove_Succeeds()
    {
        int id = await InsertEntity("/api/ProductType/Insert",
            new { value = new { ProductTypeName = "Test ProdType", Description = "Int test" } },
            "ProductTypeId");
        Assert.IsTrue(id > 0);

        await RemoveEntity("/api/ProductType/Remove", id);
    }

    // ───────────────────────── BillType ─────────────────────────

    [TestMethod]
    public async Task BillType_GetAll_ReturnsItemsAndCount()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/BillType");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        await AssertItemsAndCount(response);
    }

    [TestMethod]
    public async Task BillType_InsertAndRemove_Succeeds()
    {
        int id = await InsertEntity("/api/BillType/Insert",
            new { value = new { BillTypeName = "Test BillType", Description = "Int test" } },
            "BillTypeId");
        Assert.IsTrue(id > 0);

        await RemoveEntity("/api/BillType/Remove", id);
    }

    // ───────────────────────── InvoiceType ─────────────────────────

    [TestMethod]
    public async Task InvoiceType_GetAll_ReturnsItemsAndCount()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/InvoiceType");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        await AssertItemsAndCount(response);
    }

    [TestMethod]
    public async Task InvoiceType_InsertAndRemove_Succeeds()
    {
        int id = await InsertEntity("/api/InvoiceType/Insert",
            new { value = new { InvoiceTypeName = "Test InvType", Description = "Int test" } },
            "InvoiceTypeId");
        Assert.IsTrue(id > 0);

        await RemoveEntity("/api/InvoiceType/Remove", id);
    }

    // ───────────────────────── PaymentType ─────────────────────────

    [TestMethod]
    public async Task PaymentType_GetAll_ReturnsItemsAndCount()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/PaymentType");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        await AssertItemsAndCount(response);
    }

    [TestMethod]
    public async Task PaymentType_InsertAndRemove_Succeeds()
    {
        int id = await InsertEntity("/api/PaymentType/Insert",
            new { value = new { PaymentTypeName = "Test PayType", Description = "Int test" } },
            "PaymentTypeId");
        Assert.IsTrue(id > 0);

        await RemoveEntity("/api/PaymentType/Remove", id);
    }

    // ───────────────────────── PurchaseType ─────────────────────────

    [TestMethod]
    public async Task PurchaseType_GetAll_ReturnsItemsAndCount()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/PurchaseType");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        await AssertItemsAndCount(response);
    }

    [TestMethod]
    public async Task PurchaseType_InsertAndRemove_Succeeds()
    {
        int id = await InsertEntity("/api/PurchaseType/Insert",
            new { value = new { PurchaseTypeName = "Test PurType", Description = "Int test" } },
            "PurchaseTypeId");
        Assert.IsTrue(id > 0);

        await RemoveEntity("/api/PurchaseType/Remove", id);
    }

    // ───────────────────────── SalesType ─────────────────────────

    [TestMethod]
    public async Task SalesType_GetAll_ReturnsItemsAndCount()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/SalesType");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        await AssertItemsAndCount(response);
    }

    [TestMethod]
    public async Task SalesType_InsertAndRemove_Succeeds()
    {
        int id = await InsertEntity("/api/SalesType/Insert",
            new { value = new { SalesTypeName = "Test SaleType", Description = "Int test" } },
            "SalesTypeId");
        Assert.IsTrue(id > 0);

        await RemoveEntity("/api/SalesType/Remove", id);
    }

    // ───────────────────────── ShipmentType ─────────────────────────

    [TestMethod]
    public async Task ShipmentType_GetAll_ReturnsItemsAndCount()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/ShipmentType");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        await AssertItemsAndCount(response);
    }

    [TestMethod]
    public async Task ShipmentType_InsertAndRemove_Succeeds()
    {
        int id = await InsertEntity("/api/ShipmentType/Insert",
            new { value = new { ShipmentTypeName = "Test ShipType", Description = "Int test" } },
            "ShipmentTypeId");
        Assert.IsTrue(id > 0);

        await RemoveEntity("/api/ShipmentType/Remove", id);
    }

    // ───────────────────────── Helpers ─────────────────────────

    private static async Task AssertItemsAndCount(HttpResponseMessage response)
    {
        JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        Assert.IsTrue(doc.RootElement.TryGetProperty("Items", out JsonElement items));
        Assert.AreEqual(JsonValueKind.Array, items.ValueKind);
        Assert.IsTrue(doc.RootElement.TryGetProperty("Count", out JsonElement count));
        Assert.AreEqual(count.GetInt32(), items.GetArrayLength());
    }

    private static async Task<int> InsertEntity(string url, object payload, string idProperty)
    {
        HttpResponseMessage response = await _client.PostAsJsonAsync(url, payload);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        return doc.RootElement.GetProperty(idProperty).GetInt32();
    }

    private static async Task RemoveEntity(string url, int id)
    {
        var removePayload = new { key = id.ToString() };
        HttpResponseMessage response = await _client.PostAsJsonAsync(url, removePayload);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }
}
