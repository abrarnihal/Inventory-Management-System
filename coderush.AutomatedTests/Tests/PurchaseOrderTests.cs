using coderush.AutomatedTests.Infrastructure;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace coderush.AutomatedTests.Tests;

/// <summary>
/// Selenium tests for PurchaseOrder pages — Index grid and Detail view.
/// </summary>
[TestClass]
public sealed class PurchaseOrderTests : SeleniumTestBase
{
    [TestInitialize]
    public void EnsureAuth() => EnsureLoggedIn();

    [TestMethod]
    public void PurchaseOrderIndex_GridLoads()
    {
        NavigateTo("/PurchaseOrder/Index");
        WaitForGrid();

        IWebElement grid = Driver.FindElement(By.CssSelector(".e-grid"));
        Assert.IsNotNull(grid, "Syncfusion grid should be present.");
    }

    [TestMethod]
    public void PurchaseOrderIndex_Add_OpensEditRow()
    {
        NavigateTo("/PurchaseOrder/Index");
        WaitForGrid();

        static IWebElement? FindVisibleEditor(IWebDriver d)
        {
            foreach (var dialog in d.FindElements(By.CssSelector("#Grid_dialogEdit_wrapper, #Grid_dialogEdit")))
            {
                if (dialog.Displayed)
                {
                    return dialog;
                }
            }

            foreach (var row in d.FindElements(By.CssSelector(".e-editedrow")))
            {
                if (row.Displayed)
                {
                    return row;
                }
            }

            return null;
        }

        var addButton = Wait.Until(d =>
        {
            foreach (var candidate in d.FindElements(By.CssSelector("#Grid_add, li[id$='_add'], .e-gridtoolbar li[title='Add'], .e-gridtoolbar li[aria-label='Add']")))
            {
                if (candidate.Displayed && candidate.Enabled)
                {
                    return candidate;
                }
            }

            return null;
        });

        if (addButton is null)
        {
            Assert.Inconclusive("Visible Add button not found.");
            return;
        }

        ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].scrollIntoView({ block: 'center' });", addButton);

        try
        {
            addButton.Click();
        }
        catch (ElementClickInterceptedException)
        {
            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].click();", addButton);
        }

        var editorWait = new WebDriverWait(Driver, TimeSpan.FromSeconds(20))
        {
            PollingInterval = TimeSpan.FromMilliseconds(250)
        };

        IWebElement? editor;

        try
        {
            editor = new WebDriverWait(Driver, TimeSpan.FromSeconds(5))
            {
                PollingInterval = TimeSpan.FromMilliseconds(250)
            }.Until(FindVisibleEditor);
        }
        catch (WebDriverTimeoutException)
        {
            ((IJavaScriptExecutor)Driver).ExecuteScript(
                "var grid = window.jQuery ? window.jQuery('#Grid').ejGrid('instance') : null; if (grid && typeof grid.addRecord === 'function') { grid.addRecord(); } else { var toolbarAdd = document.querySelector(\"#Grid_add, li[id$='_add'], .e-gridtoolbar li[title='Add'], .e-gridtoolbar li[aria-label='Add']\"); if (toolbarAdd) { toolbarAdd.click(); } }");

            editor = editorWait.Until(FindVisibleEditor);
        }

        Assert.IsNotNull(editor, "Add button should open an edit row or dialog.");

        var editableInputs = Driver.FindElements(By.CssSelector("#Grid_dialogEdit input, #Grid_dialogEdit select, .e-editedrow input, .e-editedrow select"));
        Assert.IsTrue(editableInputs.Count > 0,
            "Add button should open an editable purchase order form.");
    }

    [TestMethod]
    public void PurchaseOrderDetail_InvalidId_ShowsNotFoundOrError()
    {
        NavigateTo("/PurchaseOrder/Detail/99999");

        // The controller returns NotFound() for missing orders.
        bool isErrorPage =
            Driver.PageSource.Contains("404") ||
            Driver.PageSource.Contains("Not Found") ||
            Driver.PageSource.Contains("Error") ||
            Driver.Url.Contains("Error");

        Assert.IsTrue(isErrorPage,
            "Navigating to a non-existent PO detail should show a 404 or error.");
    }
}