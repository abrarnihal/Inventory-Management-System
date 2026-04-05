using coderush.AutomatedTests.Infrastructure;
using OpenQA.Selenium;

namespace coderush.AutomatedTests.Tests;

/// <summary>
/// Selenium tests for user-management pages accessible to the super admin.
/// </summary>
[TestClass]
public sealed class UserProfileTests : SeleniumTestBase
{
    [TestInitialize]
    public void EnsureAuth() => EnsureLoggedIn();

    [TestMethod]
    public void UserProfile_DisplaysCurrentUserEmail()
    {
        NavigateTo("/UserRole/UserProfile");

        // The profile page should display the super-admin email or name.
        Assert.IsTrue(
            Driver.PageSource.Contains("super@admin.com") ||
            Driver.PageSource.Contains("Super") ||
            Driver.PageSource.Contains("Admin"),
            "User profile page should display the logged-in user's info.");
    }

    [TestMethod]
    public void UserList_GridLoads()
    {
        NavigateTo("/UserRole/Index");

        // Wait for the user grid to appear.
        Wait.Until(d => d.FindElements(By.CssSelector(".e-grid")).Count > 0);

        var grid = Driver.FindElements(By.CssSelector(".e-grid"));
        Assert.IsTrue(grid.Count > 0,
            "User list page should have a Syncfusion grid.");
    }

    [TestMethod]
    public void ChangePassword_PageLoads()
    {
        NavigateTo("/UserRole/ChangePassword");

        Assert.IsFalse(Driver.Url.Contains("/Account/Login"),
            "Change password page should be accessible to authenticated users.");
    }

    [TestMethod]
    public void RoleManagement_PageLoads()
    {
        NavigateTo("/UserRole/Role");

        Assert.IsFalse(Driver.Url.Contains("/Account/Login"),
            "Role management page should be accessible to super admin.");
    }
}
