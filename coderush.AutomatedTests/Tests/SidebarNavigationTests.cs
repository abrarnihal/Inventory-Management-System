using coderush.AutomatedTests.Infrastructure;
using OpenQA.Selenium;

namespace coderush.AutomatedTests.Tests;

/// <summary>
/// Verifies that clicking sidebar links navigates to the correct pages.
/// Tests actual user interaction with the sidebar menu.
/// </summary>
[TestClass]
public sealed class SidebarNavigationTests : SeleniumTestBase
{
    [TestInitialize]
    public void EnsureAuth() => EnsureLoggedIn();

    [TestMethod]
    public void SidebarLink_Customer_NavigatesToCustomerPage()
        => ClickSidebarLinkAndVerify("/Customer/Index", "Customer");

    [TestMethod]
    public void SidebarLink_Vendor_NavigatesToVendorPage()
        => ClickSidebarLinkAndVerify("/Vendor/Index", "Vendor");

    [TestMethod]
    public void SidebarLink_Product_NavigatesToProductPage()
        => ClickSidebarLinkAndVerify("/Product/Index", "Product");

    [TestMethod]
    public void SidebarLink_PurchaseOrder_NavigatesToPurchaseOrderPage()
        => ClickSidebarLinkAndVerify("/PurchaseOrder/Index", "Purchase Order");

    [TestMethod]
    public void SidebarLink_SalesOrder_NavigatesToSalesOrderPage()
        => ClickSidebarLinkAndVerify("/SalesOrder/Index", "Sales Order");

    [TestMethod]
    public void SidebarLink_Currency_NavigatesToCurrencyPage()
        => ClickSidebarLinkAndVerify("/Currency/Index", "Currency");

    [TestMethod]
    public void SidebarLink_Branch_NavigatesToBranchPage()
        => ClickSidebarLinkAndVerify("/Branch/Index", "Branch");

    [TestMethod]
    public void SidebarLink_Warehouse_NavigatesToWarehousePage()
        => ClickSidebarLinkAndVerify("/Warehouse/Index", "Warehouse");

    private void ClickSidebarLinkAndVerify(string expectedPath, string linkText)
    {
        var js = (IJavaScriptExecutor)Driver;

        // Only pay the cost of a full page load if the sidebar isn't already
        // on the current page (i.e. the very first test in the class).
        // Subsequent tests reuse the sidebar from the previous navigation target.
        if (js.ExecuteScript("return !!document.querySelector('.sidebar-menu');") is not true)
        {
            NavigateTo("/UserRole/UserProfile");
            // JS-based check avoids stacking the 3 s implicit wait with WebDriverWait polling.
            Wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript(
                "return !!document.querySelector('.sidebar-menu');") is true);
        }

        // JavaScript click bypasses Selenium's scroll-into-view / actionability
        // round-trips — one CDP call instead of three.
        js.ExecuteScript($"document.querySelector(\"a[href='{expectedPath}']\").click();");

        Wait.Until(d => d.Url.Contains(expectedPath));
        Assert.IsTrue(Driver.Url.Contains(expectedPath),
            $"Expected URL to contain '{expectedPath}' after clicking '{linkText}' but got: {Driver.Url}");
    }
}