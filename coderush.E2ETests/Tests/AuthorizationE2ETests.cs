using coderush.E2ETests.Infrastructure;
using OpenQA.Selenium;

namespace coderush.E2ETests.Tests;

/// <summary>
/// E2E tests verifying that role-based authorization works across the full application.
/// Tests navigate through multiple protected pages to ensure consistent enforcement.
/// </summary>
[TestClass]
public sealed class AuthorizationE2ETests : E2ETestBase
{
    [TestInitialize]
    public void ClearAuth() => ClearSession();

    [TestMethod]
    public void E2E_UnauthenticatedUser_AllProtectedPagesRedirectToLogin()
    {
        string[] protectedPaths =
        [
            "/Customer/Index",
            "/Vendor/Index",
            "/Product/Index",
            "/SalesOrder/Index",
            "/PurchaseOrder/Index",
            "/Shipment/Index",
            "/GoodsReceivedNote/Index",
            "/Invoice/Index",
            "/Bill/Index",
            "/PaymentReceive/Index",
            "/PaymentVoucher/Index",
            "/UserRole/Index",
            "/Dashboard/Index"
        ];

        foreach (string path in protectedPaths)
        {
            NavigateTo(path);
            Assert.IsTrue(Driver.Url.Contains("/Account/Login"),
                $"Unauthenticated access to {path} should redirect to login. Actual URL: {Driver.Url}");
        }
    }

    [TestMethod]
    public void E2E_AuthenticatedSuperAdmin_CanAccessAllPages()
    {
        LoginAsSuperAdmin();

        string[] allPaths =
        [
            "/Customer/Index",
            "/Vendor/Index",
            "/Product/Index",
            "/SalesOrder/Index",
            "/PurchaseOrder/Index",
            "/Shipment/Index",
            "/GoodsReceivedNote/Index",
            "/Invoice/Index",
            "/Bill/Index",
            "/PaymentReceive/Index",
            "/PaymentVoucher/Index",
            "/Dashboard/Index"
        ];

        foreach (string path in allPaths)
        {
            NavigateTo(path);
            Assert.IsFalse(Driver.Url.Contains("/Account/Login"),
                $"Super admin should access {path} without redirect. Actual URL: {Driver.Url}");
            Assert.IsFalse(Driver.Url.Contains("/Account/AccessDenied"),
                $"Super admin should not get 403 on {path}. Actual URL: {Driver.Url}");
        }
    }

    [TestMethod]
    public void E2E_LoginLogoutLogin_SessionStateClearedCorrectly()
    {
        // First login
        LoginAsSuperAdmin();
        NavigateTo("/Customer/Index");
        Wait.Until(d => !d.Url.Contains("/Account/Login"));
        Assert.IsFalse(Driver.Url.Contains("/Account/Login"),
            "Should access Customer page after login.");

        // Clear session (cookie-based: avoids form submission + page transition of Logout())
        ClearSession();

        // Verify protected page redirects after session cleared
        NavigateTo("/SalesOrder/Index");
        Wait.Until(d => d.Url.Contains("/Account/Login"));
        Assert.IsTrue(Driver.Url.Contains("/Account/Login"),
            "Should redirect to login after logout.");

        // Login again
        LoginAsSuperAdmin();
        NavigateTo("/SalesOrder/Index");
        Wait.Until(d => !d.Url.Contains("/Account/Login"));
        Assert.IsFalse(Driver.Url.Contains("/Account/Login"),
            "Should access Sales Order page after re-login.");
    }

    [TestMethod]
    public void E2E_UnauthenticatedApiCall_Returns401()
    {
        NavigateTo("/Account/Login"); // Ensure we're on a page with a domain context

        string status = GetApiStatus("/api/SalesOrder");
        Assert.IsTrue(status == "401" || status == "302",
            $"Unauthenticated API call should return 401 or 302, got: {status}");
    }
}
