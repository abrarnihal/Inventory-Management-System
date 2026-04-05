using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace coderush.E2ETests.Infrastructure.PageObjects;

/// <summary>
/// Base Page Object for any page containing a Syncfusion ejGrid.
/// Encapsulates common grid interactions: add record, edit, delete, row count, cell read.
/// </summary>
public class SyncfusionGridPage
{
    protected readonly IWebDriver Driver;
    protected readonly WebDriverWait Wait;
    protected readonly string BaseUrl;
    protected readonly string PagePath;

    public SyncfusionGridPage(IWebDriver driver, WebDriverWait wait, string baseUrl, string pagePath)
    {
        Driver = driver;
        Wait = wait;
        BaseUrl = baseUrl;
        PagePath = pagePath;
    }

    public void Navigate()
    {
        Driver.Navigate().GoToUrl($"{BaseUrl}{PagePath}");
        WaitForGrid();
    }

    public void WaitForGrid()
    {
        Wait.Until(d =>
        {
            var grids = d.FindElements(By.CssSelector(".e-grid"));
            if (grids.Count == 0) return false;
            // Wait until the Syncfusion ejGrid has been fully initialised with data.
            // The grid widget stores its row collection once the remote data load completes.
            var js = (IJavaScriptExecutor)d;
            var ready = js.ExecuteScript(
                "var g = typeof jQuery !== 'undefined' ? jQuery('#Grid').data('ejGrid') : null;" +
                "return g && g.model && g.model.currentViewData && g.model.currentViewData.length > 0;");
            return ready is true;
        });
    }

    public int GetRowCount()
    {
        return Driver.FindElements(By.CssSelector(".e-grid .e-row")).Count;
    }

    public string GetCellText(int rowIndex, int colIndex)
    {
        IWebElement cell = Driver.FindElement(
            By.CssSelector($".e-grid .e-row:nth-child({rowIndex + 1}) td:nth-child({colIndex + 1})"));
        return cell.Text;
    }

    /// <summary>
    /// Checks whether any record in the grid's data source (across all pages)
    /// contains the specified value in the given field.
    /// For remote data sources (WebApiAdaptor), the DataManager's internal
    /// json cache is empty, so this method first checks currentViewData and
    /// then falls back to fetching all records from the grid's data URL.
    /// </summary>
    public bool HasRecord(string fieldName, string value)
    {
        var escapedField = fieldName.Replace("'", "\\'");
        var escapedValue = value.Replace("'", "\\'");

        var prevTimeout = Driver.Manage().Timeouts().AsynchronousJavaScript;
        Driver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromSeconds(10);
        try
        {
            var result = ((IJavaScriptExecutor)Driver).ExecuteAsyncScript(
                "var done = arguments[arguments.length - 1];" +
                "var g = jQuery('#Grid').data('ejGrid');" +
                "if (!g || !g.model) { done(false); return; }" +
                // Quick check: is the record on the current page?
                "var cvd = g.model.currentViewData || [];" +
                $"if (cvd.some(function(r){{ return r['{escapedField}'] === '{escapedValue}'; }})) {{ done(true); return; }}" +
                "var ds = g.model.dataSource;" +
                // For remoteSaveAdaptor grids the full dataset lives in ds.dataSource.json
                "var localJson = (ds && ds.dataSource) ? ds.dataSource.json : null;" +
                $"if (localJson && localJson.some(function(r){{ return r['{escapedField}'] === '{escapedValue}'; }})) {{ done(true); return; }}" +
                // Use DataManager.executeLocal to retrieve all records (works with remoteSaveAdaptor)
                "try { var all = ds.executeLocal(ej.Query());" +
                $"  if (all && all.some(function(r){{ return r['{escapedField}'] === '{escapedValue}'; }})) {{ done(true); return; }}" +
                "} catch(ex) {}" +
                // Fall back to fetching from the remote URL.
                "var url = (ds && ds.dataSource) ? ds.dataSource.url : null;" +
                // For remoteSaveAdaptor grids, derive the base API URL from insertUrl/updateUrl.
                "if (!url && ds && ds.dataSource) {" +
                "  var iu = ds.dataSource.insertUrl || ds.dataSource.updateUrl || '';" +
                "  var li = iu.lastIndexOf('/');" +
                "  if (li > 0) url = iu.substring(0, li);" +
                "}" +
                "if (!url) { done(false); return; }" +
                "fetch(url).then(function(r){ return r.json(); }).then(function(d) {" +
                "  var items = d.Items || d.items || d || [];" +
                $"  done(items.some(function(r){{ return r['{escapedField}'] === '{escapedValue}'; }}));" +
                "}).catch(function(){ done(false); });");
            return result is true;
        }
        finally
        {
            Driver.Manage().Timeouts().AsynchronousJavaScript = prevTimeout;
        }
    }

    /// <summary>
    /// Triggers the grid's addRecord() and waits for the edit form to become visible.
    /// Returns the number of input fields in the edit form.
    /// </summary>
    public long AddRecord()
    {
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
            return result is long c ? c : 0;
        }
        finally
        {
            Driver.Manage().Timeouts().AsynchronousJavaScript = prevTimeout;
        }
    }

    /// <summary>Sets a field value in the currently open edit form via JavaScript.</summary>
    public void SetEditField(string fieldId, string value)
    {
        ((IJavaScriptExecutor)Driver).ExecuteScript(
            $"var el = document.getElementById('{fieldId}'); if(el) {{ el.value = '{value}'; el.dispatchEvent(new Event('change')); }}");
    }

    /// <summary>Saves the current edit form by triggering endEdit on the grid.</summary>
    public void SaveRecord()
    {
        ((IJavaScriptExecutor)Driver).ExecuteScript(
            "var grid = jQuery('#Grid').data('ejGrid'); if(grid) grid.endEdit();");
        Wait.Until(d => d.FindElements(By.CssSelector(".e-editedrow")).Count == 0);
    }

    /// <summary>Deletes the first row via the grid's deleteRecord API.</summary>
    public void DeleteFirstRecord()
    {
        ((IJavaScriptExecutor)Driver).ExecuteScript(
            "var grid = jQuery('#Grid').data('ejGrid'); if(grid) grid.deleteRecord();");
    }
}
