using coderush.AutomatedTests.Infrastructure;
using OpenQA.Selenium;

namespace coderush.AutomatedTests.Tests;

/// <summary>
/// Verifies that every page accessible from the sidebar navigation
/// loads successfully for an authenticated super-admin user.
/// </summary>
[TestClass]
public sealed class NavigationTests : SeleniumTestBase
{
    [TestInitialize]
    public void EnsureAuth() => EnsureLoggedIn();

    // ────────── SALES section ──────────

    [TestMethod]
    public void Navigate_CustomerType_PageLoads()
        => AssertPageLoads("/CustomerType/Index", "Customer Type");

    [TestMethod]
    public void Navigate_Customer_PageLoads()
        => AssertPageLoads("/Customer/Index", "Customer");

    [TestMethod]
    public void Navigate_SalesType_PageLoads()
        => AssertPageLoads("/SalesType/Index", "Sales Type");

    [TestMethod]
    public void Navigate_SalesOrder_PageLoads()
        => AssertPageLoads("/SalesOrder/Index", "Sales Order");

    [TestMethod]
    public void Navigate_Shipment_PageLoads()
        => AssertPageLoads("/Shipment/Index", "Shipment");

    [TestMethod]
    public void Navigate_Invoice_PageLoads()
        => AssertPageLoads("/Invoice/Index", "Invoice");

    [TestMethod]
    public void Navigate_PaymentReceive_PageLoads()
        => AssertPageLoads("/PaymentReceive/Index", "Payment Receive");

    // ────────── PURCHASE section ──────────

    [TestMethod]
    public void Navigate_VendorType_PageLoads()
        => AssertPageLoads("/VendorType/Index", "Vendor Type");

    [TestMethod]
    public void Navigate_Vendor_PageLoads()
        => AssertPageLoads("/Vendor/Index", "Vendor");

    [TestMethod]
    public void Navigate_PurchaseType_PageLoads()
        => AssertPageLoads("/PurchaseType/Index", "Purchase Type");

    [TestMethod]
    public void Navigate_PurchaseOrder_PageLoads()
        => AssertPageLoads("/PurchaseOrder/Index", "Purchase Order");

    [TestMethod]
    public void Navigate_GoodsReceivedNote_PageLoads()
        => AssertPageLoads("/GoodsReceivedNote/Index", "Goods Received Note");

    [TestMethod]
    public void Navigate_Bill_PageLoads()
        => AssertPageLoads("/Bill/Index", "Bill");

    [TestMethod]
    public void Navigate_PaymentVoucher_PageLoads()
        => AssertPageLoads("/PaymentVoucher/Index", "Payment Voucher");

    // ────────── INVENTORY section ──────────

    [TestMethod]
    public void Navigate_Product_PageLoads()
        => AssertPageLoads("/Product/Index", "Product");

    [TestMethod]
    public void Navigate_ProductType_PageLoads()
        => AssertPageLoads("/ProductType/Index", "Product Type");

    [TestMethod]
    public void Navigate_UnitOfMeasure_PageLoads()
        => AssertPageLoads("/UnitOfMeasure/Index", "Unit Of Measure");

    // ────────── CONFIG section ──────────

    [TestMethod]
    public void Navigate_Currency_PageLoads()
        => AssertPageLoads("/Currency/Index", "Currency");

    [TestMethod]
    public void Navigate_Branch_PageLoads()
        => AssertPageLoads("/Branch/Index", "Branch");

    [TestMethod]
    public void Navigate_Warehouse_PageLoads()
        => AssertPageLoads("/Warehouse/Index", "Warehouse");

    [TestMethod]
    public void Navigate_CashBank_PageLoads()
        => AssertPageLoads("/CashBank/Index", "Cash Bank");

    [TestMethod]
    public void Navigate_PaymentType_PageLoads()
        => AssertPageLoads("/PaymentType/Index", "Payment Type");

    [TestMethod]
    public void Navigate_ShipmentType_PageLoads()
        => AssertPageLoads("/ShipmentType/Index", "Shipment Type");

    [TestMethod]
    public void Navigate_InvoiceType_PageLoads()
        => AssertPageLoads("/InvoiceType/Index", "Invoice Type");

    [TestMethod]
    public void Navigate_BillType_PageLoads()
        => AssertPageLoads("/BillType/Index", "Bill Type");

    // ────────── USER & ROLE section ──────────

    [TestMethod]
    public void Navigate_UserProfile_PageLoads()
    {
        NavigateTo("/UserRole/UserProfile");
        Assert.IsFalse(Driver.Url.Contains("/Account/Login"),
            "Should not redirect to login — user is authenticated.");
    }

    // ────────── Helper ──────────

    private void AssertPageLoads(string path, string expectedText)
    {
        NavigateTo(path);

        string currentUrl;
        string pageSource;
        try
        {
            currentUrl = Driver.Url;
            pageSource = Driver.PageSource;
        }
        catch (WebDriverException)
        {
            // Chrome session died after navigation (e.g. crash under parallel load) — recover and retry.
            EnsureLoggedIn();
            NavigateTo(path);
            currentUrl = Driver.Url;
            pageSource = Driver.PageSource;
        }

        // Should NOT be redirected to login.
        Assert.IsFalse(currentUrl.Contains("/Account/Login"),
            $"Navigating to {path} redirected to login.");

        // The sidebar should contain the link for this page.
        Assert.IsTrue(pageSource.Contains(expectedText),
            $"Page at {path} does not contain expected text '{expectedText}'.");
    }
}