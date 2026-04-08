using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace coderush.IntegrationTests.Infrastructure;

/// <summary>
/// Handles the ASP.NET Core anti-forgery + captcha login flow so that
/// integration tests can obtain an authenticated <see cref="HttpClient"/>.
/// </summary>
public static partial class AuthHelper
{
    /// <summary>
    /// The super-admin credentials seeded by <c>DbInitializer</c>.
    /// </summary>
    public const string SuperAdminEmail = "super@admin.com";
    public const string SuperAdminPassword = "123456";

    /// <summary>
    /// Creates an <see cref="HttpClient"/> from the given factory that
    /// follows redirects manually so we can inspect intermediate responses,
    /// then logs in as the super-admin user.
    /// </summary>
    public static async Task<HttpClient> CreateAuthenticatedClientAsync(
        CustomWebApplicationFactory factory)
    {
        HttpClient client = factory.CreateClient(new()
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });

        await LoginAsync(client, SuperAdminEmail, SuperAdminPassword);
        return client;
    }

    /// <summary>
    /// Performs the full login flow:
    /// 1. GET /Account/Login  → extract anti-forgery token + cookie
    /// 2. POST /Account/Login → send credentials with tokens
    /// </summary>
    public static async Task LoginAsync(HttpClient client, string email, string password)
    {
        // Step 1: GET the login page to obtain the anti-forgery token.
        HttpResponseMessage getResponse = await client.GetAsync("/Account/Login");
        string html = await getResponse.Content.ReadAsStringAsync();

        string antiForgeryToken = ExtractAntiForgeryToken(html);
        string? antiForgeryCookieValue = ExtractAntiForgeryCookie(getResponse);

        // Step 2: POST the login form with credentials, captcha token, and anti-forgery token.
        var formData = new Dictionary<string, string>
        {
            ["Email"] = email,
            ["Password"] = password,
            ["RememberMe"] = "true",
            ["CaptchaToken"] = TestCaptchaService.TestToken,
            ["__RequestVerificationToken"] = antiForgeryToken
        };

        HttpRequestMessage request = new(HttpMethod.Post, "/Account/Login")
        {
            Content = new FormUrlEncodedContent(formData)
        };

        // Include the anti-forgery cookie in the request.
        if (antiForgeryCookieValue is not null)
        {
            request.Headers.Add("Cookie", antiForgeryCookieValue);
        }

        HttpResponseMessage postResponse = await client.SendAsync(request);

        // A successful login redirects (302). Follow the redirect manually.
        if (postResponse.StatusCode == HttpStatusCode.Redirect)
        {
            Uri? location = postResponse.Headers.Location;
            if (location is not null)
            {
                await client.GetAsync(location);
            }
        }
    }

    /// <summary>
    /// Extracts the <c>__RequestVerificationToken</c> hidden-field value from rendered HTML.
    /// </summary>
    public static string ExtractAntiForgeryToken(string html)
    {
        Match match = AntiForgeryTokenRegex().Match(html);
        if (!match.Success)
            throw new InvalidOperationException(
                "Could not find __RequestVerificationToken in the login page HTML.");

        return match.Groups[1].Value;
    }

    private static string? ExtractAntiForgeryCookie(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out IEnumerable<string>? cookies))
            return null;

        foreach (string cookie in cookies)
        {
            if (cookie.StartsWith(".AspNetCore.Antiforgery", StringComparison.OrdinalIgnoreCase))
            {
                // Return just the cookie name=value pair (before the first semicolon).
                int semicolonIndex = cookie.IndexOf(';');
                return semicolonIndex > 0 ? cookie[..semicolonIndex] : cookie;
            }
        }

        return null;
    }

    [GeneratedRegex(
        """name="__RequestVerificationToken"[^>]*value="([^"]+)""",
        RegexOptions.Compiled)]
    private static partial Regex AntiForgeryTokenRegex();
}
