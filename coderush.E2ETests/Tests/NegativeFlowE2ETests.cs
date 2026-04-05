using coderush.E2ETests.Infrastructure;
using OpenQA.Selenium;

namespace coderush.E2ETests.Tests;

/// <summary>
/// E2E tests for error paths, invalid data, and boundary conditions
/// that span multiple system layers.
/// </summary>
[TestClass]
public sealed class NegativeFlowE2ETests : E2ETestBase
{
    [TestInitialize]
    public void Setup() => EnsureLoggedIn();

    [TestMethod]
    public void E2E_SalesOrderDetail_NonExistentId_ShowsError()
    {
        NavigateTo("/SalesOrder/Detail/999999");

        bool isError =
            Driver.PageSource.Contains("404") ||
            Driver.PageSource.Contains("Not Found") ||
            Driver.PageSource.Contains("Error") ||
            Driver.Url.Contains("Error");

        Assert.IsTrue(isError,
            "Non-existent Sales Order detail should show 404 or error page.");
    }

    [TestMethod]
    public void E2E_PurchaseOrderDetail_NonExistentId_ShowsError()
    {
        NavigateTo("/PurchaseOrder/Detail/999999");

        bool isError =
            Driver.PageSource.Contains("404") ||
            Driver.PageSource.Contains("Not Found") ||
            Driver.PageSource.Contains("Error") ||
            Driver.Url.Contains("Error");

        Assert.IsTrue(isError,
            "Non-existent Purchase Order detail should show 404 or error page.");
    }

    [TestMethod]
    public void E2E_InvalidRoute_Returns404OrError()
    {
        NavigateTo("/ThisPageDoesNotExist/AtAll");

        bool isError =
            Driver.PageSource.Contains("404") ||
            Driver.PageSource.Contains("Error") ||
            Driver.PageSource.Contains("Not Found");

        Assert.IsTrue(isError, "Invalid route should show an error page.");
    }

    [TestMethod]
    public void E2E_ApiInsert_NullPayload_ReturnsBadRequest()
    {
        string status = PostApiAndGetStatus("/api/SalesOrder/Insert", """{"value":null}""");
        Assert.AreEqual("400", status,
            "API should return 400 for null payload.");
    }

    [TestMethod]
    public void E2E_NavigateAllConfigPages_NoServerErrors()
    {
        string[] configPaths =
        [
            "/BillType/Index",
            "/Branch/Index",
            "/CashBank/Index",
            "/Currency/Index",
            "/CustomerType/Index",
            "/InvoiceType/Index",
            "/PaymentType/Index",
            "/ProductType/Index",
            "/SalesType/Index",
            "/ShipmentType/Index",
            "/UnitOfMeasure/Index",
            "/VendorType/Index",
            "/PurchaseType/Index",
            "/Warehouse/Index"
        ];

        foreach (string path in configPaths)
        {
            NavigateTo(path);
            Assert.IsFalse(
                Driver.PageSource.Contains("An unhandled exception") ||
                Driver.PageSource.Contains("500 Internal Server Error"),
                $"Page {path} should not throw a server error.");
        }
    }
}
