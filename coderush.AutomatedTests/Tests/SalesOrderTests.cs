using coderush.AutomatedTests.Infrastructure;
using OpenQA.Selenium;

namespace coderush.AutomatedTests.Tests;

/// <summary>
/// Selenium tests for SalesOrder pages — Index grid and Detail view.
/// </summary>
[TestClass]
public sealed class SalesOrderTests : SeleniumTestBase
{
    [TestInitialize]
    public void EnsureAuth() => EnsureLoggedIn();

    [TestMethod]
    public void SalesOrderIndex_GridLoads()
    {
        NavigateTo("/SalesOrder/Index");
        WaitForGrid();

        IWebElement grid = Driver.FindElement(By.CssSelector(".e-grid"));
        Assert.IsNotNull(grid, "Syncfusion grid should be present.");
    }

    [TestMethod]
    public void SalesOrderIndex_Add_OpensEditRow()
    {
        NavigateTo("/SalesOrder/Index");

        // Single async script that polls inside the browser and calls back
        // once the edit form is visible.  This replaces the C#-side
        // WebDriverWait loop whose repeated ExecuteScript round-trips
        // added ~1.5 s of overhead.
        const string asyncJs = """
            var done = arguments[arguments.length - 1];
            var iv = setInterval(function() {
                if (typeof jQuery === 'undefined') return;
                var grid = jQuery('#Grid').data('ejGrid');
                if (!grid) return;
                var w = jQuery('#Grid_dialogEdit_wrapper');
                if (w.length && w.is(':visible')) {
                    var n = w.find('input, select, textarea').length;
                    if (n > 0) { clearInterval(iv); done(n); return; }
                }
                var r = document.querySelector('.e-editedrow');
                if (r) {
                    var n = r.querySelectorAll('input, select, textarea').length;
                    if (n > 0) { clearInterval(iv); done(n); return; }
                }
                if (!grid.model.isEdit) {
                    try { grid.addRecord(); } catch(e) {}
                }
            }, 100);
            """;

        var prevTimeout = Driver.Manage().Timeouts().AsynchronousJavaScript;
        Driver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromSeconds(20);
        try
        {
            object? result = ((IJavaScriptExecutor)Driver).ExecuteAsyncScript(asyncJs);
            long inputCount = result is long c ? c : 0;

            Assert.IsTrue(inputCount > 0,
                "Add button should open an editable sales order form.");
        }
        finally
        {
            Driver.Manage().Timeouts().AsynchronousJavaScript = prevTimeout;
        }
    }

    [TestMethod]
    public void SalesOrderDetail_InvalidId_ShowsNotFoundOrError()
    {
        NavigateTo("/SalesOrder/Detail/99999");

        bool isErrorPage =
            Driver.PageSource.Contains("404") ||
            Driver.PageSource.Contains("Not Found") ||
            Driver.PageSource.Contains("Error") ||
            Driver.Url.Contains("Error");

        Assert.IsTrue(isErrorPage,
            "Navigating to a non-existent SO detail should show a 404 or error.");
    }
}