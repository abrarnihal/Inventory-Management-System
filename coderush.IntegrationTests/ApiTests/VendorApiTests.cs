using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using coderush.IntegrationTests.Infrastructure;

namespace coderush.IntegrationTests.ApiTests;

[TestClass]
public sealed class VendorApiTests
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
    public async Task GetVendor_ReturnsItemsAndCount()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/Vendor");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        Assert.IsTrue(doc.RootElement.TryGetProperty("Items", out JsonElement items));
        Assert.AreEqual(JsonValueKind.Array, items.ValueKind);
        Assert.IsTrue(doc.RootElement.GetProperty("Count").GetInt32() >= 0);
    }

    [TestMethod]
    public async Task InsertVendor_ValidPayload_ReturnsCreatedVendor()
    {
        var payload = new
        {
            value = new
            {
                VendorName = "Integration Test Vendor",
                VendorTypeId = 1,
                Address = "456 Vendor Lane",
                City = "VendorCity",
                State = "VS",
                ZipCode = "33333",
                Phone = "555-0300",
                Email = "inttest@vendor.com",
                ContactPerson = "Vendor Contact"
            }
        };

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/Vendor/Insert", payload);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        Assert.IsTrue(doc.RootElement.GetProperty("VendorId").GetInt32() > 0);
        Assert.AreEqual("Integration Test Vendor",
            doc.RootElement.GetProperty("VendorName").GetString());
    }

    [TestMethod]
    public async Task UpdateVendor_ValidPayload_ReturnsUpdatedVendor()
    {
        var insertPayload = new { value = new { VendorName = "Vendor Before", VendorTypeId = 1 } };
        HttpResponseMessage insertResponse = await _client.PostAsJsonAsync("/api/Vendor/Insert", insertPayload);
        JsonDocument insertDoc = await JsonDocument.ParseAsync(await insertResponse.Content.ReadAsStreamAsync());
        int vendorId = insertDoc.RootElement.GetProperty("VendorId").GetInt32();

        var updatePayload = new
        {
            value = new
            {
                VendorId = vendorId,
                VendorName = "Vendor After",
                VendorTypeId = 1,
                Address = "Updated Address"
            }
        };

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/Vendor/Update", updatePayload);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        Assert.AreEqual("Vendor After", doc.RootElement.GetProperty("VendorName").GetString());
    }

    [TestMethod]
    public async Task RemoveVendor_ValidKey_ReturnsRemovedVendor()
    {
        var insertPayload = new { value = new { VendorName = "Vendor To Remove", VendorTypeId = 1 } };
        HttpResponseMessage insertResponse = await _client.PostAsJsonAsync("/api/Vendor/Insert", insertPayload);
        JsonDocument insertDoc = await JsonDocument.ParseAsync(await insertResponse.Content.ReadAsStreamAsync());
        int vendorId = insertDoc.RootElement.GetProperty("VendorId").GetInt32();

        var removePayload = new { key = vendorId.ToString() };
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/Vendor/Remove", removePayload);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }
}
