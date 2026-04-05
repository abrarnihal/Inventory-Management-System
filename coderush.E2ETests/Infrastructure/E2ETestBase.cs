using OpenQA.Selenium;
using System.Text.RegularExpressions;

namespace coderush.E2ETests.Infrastructure;

/// <summary>
/// Extends <see cref="SeleniumTestBase"/> with API helper methods
/// used by E2E tests to create/read data via the JSON API layer.
/// </summary>
[TestClass]
public abstract class E2ETestBase : SeleniumTestBase
{
    /// <summary>Returns the "Count" value from a GET API response.</summary>
    protected int GetApiCount(string apiPath)
    {
        string json = GetApiJson(apiPath);
        Match match = Regex.Match(json, @"""Count""\s*:\s*(\d+)");
        return match.Success ? int.Parse(match.Groups[1].Value) : 0;
    }

    /// <summary>Returns the highest ID value from a GET API response.</summary>
    protected int GetLatestEntityId(string apiPath, string idFieldName)
    {
        string json = GetApiJson(apiPath);
        MatchCollection matches = Regex.Matches(json, $@"""{idFieldName}""\s*:\s*(\d+)");
        return matches.Count > 0
            ? matches.Cast<Match>().Max(m => int.Parse(m.Groups[1].Value))
            : 0;
    }

    /// <summary>GETs an API endpoint and returns the raw page source (JSON).</summary>
    protected string GetApiJson(string apiPath)
    {
        Driver.Navigate().GoToUrl($"{BaseUrl}{apiPath}");
        Wait.Until(d => d.PageSource.Contains("{"));
        return Driver.PageSource;
    }

    /// <summary>POSTs a JSON body to an API endpoint using the browser's fetch API.</summary>
    protected void PostApiRecord(string apiPath, string jsonBody)
    {
        // Escape for embedding in a JS template literal
        string escapedBody = jsonBody
            .Replace("\\", "\\\\")
            .Replace("'", "\\'")
            .Replace("\n", "")
            .Replace("\r", "");

        string script = $$"""
            var done = arguments[arguments.length - 1];
            fetch('{{apiPath}}', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: '{{escapedBody}}'
            })
            .then(r => r.json())
            .then(d => done(JSON.stringify(d)))
            .catch(e => done('ERROR:' + e.message));
            """;

        var prevTimeout = Driver.Manage().Timeouts().AsynchronousJavaScript;
        Driver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromSeconds(15);
        try
        {
            ((IJavaScriptExecutor)Driver).ExecuteAsyncScript(script);
        }
        finally
        {
            Driver.Manage().Timeouts().AsynchronousJavaScript = prevTimeout;
        }
    }

    /// <summary>POSTs and returns the HTTP status code as a string.</summary>
    protected string PostApiAndGetStatus(string apiPath, string jsonBody)
    {
        string escapedBody = jsonBody
            .Replace("\\", "\\\\")
            .Replace("'", "\\'")
            .Replace("\n", "")
            .Replace("\r", "");

        string script = $$"""
            var done = arguments[arguments.length - 1];
            fetch('{{apiPath}}', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: '{{escapedBody}}'
            })
            .then(r => done(r.status.toString()))
            .catch(e => done('ERROR:' + e.message));
            """;

        var prevTimeout = Driver.Manage().Timeouts().AsynchronousJavaScript;
        Driver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromSeconds(10);
        try
        {
            object? result = ((IJavaScriptExecutor)Driver).ExecuteAsyncScript(script);
            return result?.ToString() ?? "";
        }
        finally
        {
            Driver.Manage().Timeouts().AsynchronousJavaScript = prevTimeout;
        }
    }

    /// <summary>GETs an API endpoint using fetch and returns the HTTP status code.</summary>
    protected string GetApiStatus(string apiPath)
    {
        string script = $$"""
            var done = arguments[arguments.length - 1];
            fetch('{{apiPath}}', {
                method: 'GET',
                credentials: 'same-origin'
            })
            .then(r => done(r.status.toString()))
            .catch(e => done('ERROR:' + e.message));
            """;

        var prevTimeout = Driver.Manage().Timeouts().AsynchronousJavaScript;
        Driver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromSeconds(10);
        try
        {
            object? result = ((IJavaScriptExecutor)Driver).ExecuteAsyncScript(script);
            return result?.ToString() ?? "";
        }
        finally
        {
            Driver.Manage().Timeouts().AsynchronousJavaScript = prevTimeout;
        }
    }
}
