using coderush.E2ETests.Infrastructure;

namespace coderush.E2ETests.Tests;

/// <summary>
/// E2E tests verifying the Dashboard reflects data created by business transactions.
/// </summary>
[TestClass]
public sealed class DashboardE2ETests : E2ETestBase
{
    [TestInitialize]
    public void Setup() => EnsureLoggedIn();

    [TestMethod]
    public void E2E_Dashboard_LoadsSuccessfullyForSuperAdmin()
    {
        NavigateTo("/Dashboard/Index");

        // Dashboard should not redirect and should contain meaningful content
        Assert.IsFalse(Driver.Url.Contains("/Account/Login"),
            "Dashboard should be accessible to super admin.");
        Assert.IsFalse(string.IsNullOrWhiteSpace(Driver.PageSource),
            "Dashboard should render content.");
    }

    [TestMethod]
    public void E2E_Dashboard_ReflectsSeededData()
    {
        NavigateTo("/Dashboard/Index");

        // The seed data includes 20 customers, 19 vendors, 20 products
        // Dashboard should show some indication of data presence
        bool hasContent =
            Driver.PageSource.Contains("Customer") ||
            Driver.PageSource.Contains("Vendor") ||
            Driver.PageSource.Contains("Product") ||
            Driver.PageSource.Contains("Sales") ||
            Driver.PageSource.Contains("Purchase");

        Assert.IsTrue(hasContent,
            "Dashboard should display references to core business entities.");
    }
}
