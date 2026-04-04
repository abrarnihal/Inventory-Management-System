using System.Net;
using coderush.IntegrationTests.Infrastructure;

namespace coderush.IntegrationTests.ApiTests;

[TestClass]
public sealed class AuthenticationApiTests
{
    private static CustomWebApplicationFactory _factory = null!;

    [ClassInitialize]
    public static void ClassInit(TestContext _) => _factory = new CustomWebApplicationFactory();

    [ClassCleanup]
    public static void ClassCleanup() => _factory?.Dispose();

    [TestMethod]
    public async Task Login_ValidCredentials_Redirects()
    {
        HttpClient client = _factory.CreateClient(new() { AllowAutoRedirect = false, HandleCookies = true });

        await AuthHelper.LoginAsync(client, AuthHelper.SuperAdminEmail, AuthHelper.SuperAdminPassword);

        // After login the cookie jar should contain the identity cookie.
        // Verify by accessing a protected API endpoint.
        HttpResponseMessage response = await client.GetAsync("/api/Customer");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task Login_InvalidPassword_ReturnsLoginPage()
    {
        HttpClient client = _factory.CreateClient(new() { AllowAutoRedirect = false, HandleCookies = true });

        HttpResponseMessage getResponse = await client.GetAsync("/Account/Login");
        string html = await getResponse.Content.ReadAsStringAsync();
        string token = AuthHelper.ExtractAntiForgeryToken(html);

        var formData = new Dictionary<string, string>
        {
            ["Email"] = AuthHelper.SuperAdminEmail,
            ["Password"] = "WrongPassword!",
            ["RememberMe"] = "false",
            ["CaptchaToken"] = TestCaptchaService.TestToken,
            ["__RequestVerificationToken"] = token
        };

        HttpResponseMessage response = await client.PostAsync("/Account/Login",
            new FormUrlEncodedContent(formData));

        // Invalid login re-renders the form (200) with an error message.
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        string body = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(body.Contains("Invalid login attempt"));
    }

    [TestMethod]
    public async Task ProtectedEndpoint_Unauthenticated_RedirectsToLogin()
    {
        HttpClient client = _factory.CreateClient(new() { AllowAutoRedirect = false, HandleCookies = true });

        HttpResponseMessage response = await client.GetAsync("/api/Customer");

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
