using coderush.E2ETests.Infrastructure;
using coderush.E2ETests.Infrastructure.PageObjects;
using OpenQA.Selenium;
using System.Text.RegularExpressions;

namespace coderush.E2ETests.Tests;

/// <summary>
/// E2E tests verifying that master data (Customer, Vendor, Product) CRUD operations
/// propagate correctly through the UI grids and are available for downstream transactions.
/// </summary>
[TestClass]
public sealed class MasterDataE2ETests : E2ETestBase
{
    [TestInitialize]
    public void Setup() => EnsureLoggedIn();

    [TestMethod]
    public void E2E_Customer_CreateAndVerifyInGrid()
    {
        PostApiRecord("/api/Customer/Insert", """
            {"value":{"CustomerName":"E2E Test Corp","Address":"123 Test St","CustomerTypeId":1}}
            """);

        var page = new SyncfusionGridPage(Driver, Wait, BaseUrl, "/Customer/Index");
        page.Navigate();

        Assert.IsTrue(page.HasRecord("CustomerName", "E2E Test Corp"),
            "Newly created customer should appear in the grid.");
    }

    [TestMethod]
    public void E2E_Vendor_CreateAndVerifyInGrid()
    {
        PostApiRecord("/api/Vendor/Insert", """
            {"value":{"VendorName":"E2E Vendor LLC","Address":"456 Vendor Ave","VendorTypeId":1}}
            """);

        var page = new SyncfusionGridPage(Driver, Wait, BaseUrl, "/Vendor/Index");
        page.Navigate();

        Assert.IsTrue(page.HasRecord("VendorName", "E2E Vendor LLC"),
            "Newly created vendor should appear in the grid.");
    }

    [TestMethod]
    public void E2E_Product_CreateAndVerifyInGrid()
    {
        PostApiRecord("/api/Product/Insert", """
            {"value":{"ProductName":"E2E Widget","ProductTypeId":1,"UnitOfMeasureId":1,"DefaultBuyingPrice":10.00,"DefaultSellingPrice":25.00}}
            """);

        var page = new SyncfusionGridPage(Driver, Wait, BaseUrl, "/Product/Index");
        page.Navigate();

        Assert.IsTrue(page.HasRecord("ProductName", "E2E Widget"),
            "Newly created product should appear in the grid.");
    }

    [TestMethod]
    public void E2E_Customer_CreateThenUseInSalesOrder()
    {
        PostApiRecord("/api/Customer/Insert", """
            {"value":{"CustomerName":"E2E Chain Customer","Address":"789 Chain Rd","CustomerTypeId":1}}
            """);

        string custJson = GetApiJson("/api/Customer");
        MatchCollection custMatches = Regex.Matches(custJson, @"""CustomerId""\s*:\s*(\d+)");
        int custId = custMatches.Cast<Match>().Max(m => int.Parse(m.Groups[1].Value));

        PostApiRecord("/api/SalesOrder/Insert",
            $$$"""{"value":{"OrderDate":"2026-03-31","DeliveryDate":"2026-04-14","CustomerId":{{{custId}}},"SalesTypeId":1,"BranchId":1,"CurrencyId":1,"Remarks":"Order for E2E Chain Customer","Freight":0}}""");

        string soJson = GetApiJson("/api/SalesOrder");
        Assert.IsTrue(soJson.Contains("Order for E2E Chain Customer"),
            "Sales Order should be created for the new customer.");
    }
}