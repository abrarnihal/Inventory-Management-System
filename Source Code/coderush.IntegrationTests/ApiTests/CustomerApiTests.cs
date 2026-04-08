using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using coderush.IntegrationTests.Infrastructure;

namespace coderush.IntegrationTests.ApiTests;

[TestClass]
public sealed class CustomerApiTests
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
    public async Task GetCustomer_ReturnsItemsAndCount()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/Customer");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        Assert.IsTrue(doc.RootElement.TryGetProperty("Items", out JsonElement items));
        Assert.AreEqual(JsonValueKind.Array, items.ValueKind);
        Assert.IsTrue(doc.RootElement.TryGetProperty("Count", out JsonElement count));
        Assert.IsTrue(count.GetInt32() >= 0);
        Assert.AreEqual(count.GetInt32(), items.GetArrayLength());
    }

    [TestMethod]
    public async Task InsertCustomer_ValidPayload_ReturnsCreatedCustomer()
    {
        var payload = new
        {
            value = new
            {
                CustomerName = "Integration Test Customer",
                CustomerTypeId = 1,
                Address = "123 Test Street",
                City = "TestCity",
                State = "TS",
                ZipCode = "00000",
                Phone = "555-0199",
                Email = "inttest@customer.com",
                ContactPerson = "Test Person"
            }
        };

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/Customer/Insert", payload);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        Assert.IsTrue(doc.RootElement.GetProperty("CustomerId").GetInt32() > 0);
        Assert.AreEqual("Integration Test Customer",
            doc.RootElement.GetProperty("CustomerName").GetString());
    }

    [TestMethod]
    public async Task UpdateCustomer_ValidPayload_ReturnsUpdatedCustomer()
    {
        // Insert first.
        var insertPayload = new
        {
            value = new
            {
                CustomerName = "Before Update",
                CustomerTypeId = 1,
                Address = "Old Address",
                City = "OldCity",
                State = "OC",
                ZipCode = "11111",
                Phone = "555-0001",
                Email = "before@update.com",
                ContactPerson = "Old Person"
            }
        };
        HttpResponseMessage insertResponse = await _client.PostAsJsonAsync("/api/Customer/Insert", insertPayload);
        JsonDocument insertDoc = await JsonDocument.ParseAsync(await insertResponse.Content.ReadAsStreamAsync());
        int customerId = insertDoc.RootElement.GetProperty("CustomerId").GetInt32();

        // Update.
        var updatePayload = new
        {
            value = new
            {
                CustomerId = customerId,
                CustomerName = "After Update",
                CustomerTypeId = 1,
                Address = "New Address",
                City = "NewCity",
                State = "NC",
                ZipCode = "22222",
                Phone = "555-0002",
                Email = "after@update.com",
                ContactPerson = "New Person"
            }
        };

        HttpResponseMessage updateResponse = await _client.PostAsJsonAsync("/api/Customer/Update", updatePayload);

        Assert.AreEqual(HttpStatusCode.OK, updateResponse.StatusCode);
        JsonDocument updateDoc = await JsonDocument.ParseAsync(await updateResponse.Content.ReadAsStreamAsync());
        Assert.AreEqual("After Update",
            updateDoc.RootElement.GetProperty("CustomerName").GetString());
    }

    [TestMethod]
    public async Task RemoveCustomer_ValidKey_ReturnsRemovedCustomer()
    {
        // Insert first.
        var insertPayload = new
        {
            value = new
            {
                CustomerName = "To Be Removed",
                CustomerTypeId = 1,
                Address = "Remove St"
            }
        };
        HttpResponseMessage insertResponse = await _client.PostAsJsonAsync("/api/Customer/Insert", insertPayload);
        JsonDocument insertDoc = await JsonDocument.ParseAsync(await insertResponse.Content.ReadAsStreamAsync());
        int customerId = insertDoc.RootElement.GetProperty("CustomerId").GetInt32();

        // Remove.
        var removePayload = new { key = customerId.ToString() };

        HttpResponseMessage removeResponse = await _client.PostAsJsonAsync("/api/Customer/Remove", removePayload);

        Assert.AreEqual(HttpStatusCode.OK, removeResponse.StatusCode);
    }
}
