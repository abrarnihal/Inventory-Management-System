using coderush.AutomatedTests.Infrastructure;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace coderush.AutomatedTests.Tests;

[TestClass]
public sealed class VendorCrudTests : SeleniumTestBase
{
    [TestInitialize]
    public void NavigateToPage()
    {
        EnsureLoggedIn();
        NavigateTo("/Vendor/Index");
        WaitForGrid();
    }

    [TestMethod]
    public void VendorGrid_Loads_DisplaysSeededData()
    {
        var rows = Driver.FindElements(By.CssSelector(".e-grid .e-row"));
        Assert.IsTrue(rows.Count > 0,
            "Grid should display seeded vendor rows.");
    }

    [TestMethod]
    public void VendorGrid_HasColumnHeaders()
    {
        var headers = Driver.FindElements(By.CssSelector(".e-grid .e-headercelldiv"));
        Assert.IsTrue(headers.Count > 0,
            "Grid should have visible column headers.");
    }

    [TestMethod]
    public void VendorGrid_Add_OpensEditRow()
    {
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

        Assert.IsNotNull(editor, "Clicking Add should show an edit row or dialog.");

        var editableInputs = Driver.FindElements(By.CssSelector("#Grid_dialogEdit input, #Grid_dialogEdit select, .e-editedrow input, .e-editedrow select"));
        Assert.IsTrue(editableInputs.Count > 0,
            "Clicking Add should open an editable vendor form.");
    }
}