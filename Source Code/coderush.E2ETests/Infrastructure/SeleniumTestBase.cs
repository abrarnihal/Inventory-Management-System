using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;

[assembly: Parallelize(Workers = 4, Scope = ExecutionScope.ClassLevel)]

namespace coderush.E2ETests.Infrastructure;

/// <summary>
/// Base class for all E2E Selenium test classes.
///
/// Performance optimizations applied:
/// <list type="bullet">
///   <item>One <see cref="ChromeDriverService"/> (chromedriver.exe) per assembly — avoids re-launching the process.</item>
///   <item>One <see cref="IWebDriver"/> per test <em>class</em> — avoids ~2 sec Chrome startup per test.</item>
///   <item>One login per class via <see cref="EnsureLoggedIn"/> — avoids repeated form submissions.</item>
///   <item><see cref="PageLoadStrategy.Eager"/> — continues once DOM is interactive, skips waiting for images/CSS.</item>
///   <item>Images disabled, extensions disabled, DNS prefetch disabled — faster page loads.</item>
///   <item>Parallel class execution via <c>[assembly: Parallelize]</c>.</item>
///   <item>Reduced implicit wait (3 s) and explicit wait (10 s) with 250 ms polling.</item>
/// </list>
/// </summary>
[TestClass]
public abstract class SeleniumTestBase
{
    // ─── Assembly-wide shared resources ───
    private static TestWebServer _server = null!;
    private static ChromeDriverService _driverService = null!;
    protected static string BaseUrl { get; private set; } = "";

    // ─── Per test-class (ThreadStatic for parallel class execution) ───
    [ThreadStatic] private static IWebDriver? _driver;
    [ThreadStatic] private static WebDriverWait? _wait;
    [ThreadStatic] private static bool _isLoggedIn;

    protected IWebDriver Driver => _driver!;
    protected WebDriverWait Wait => _wait!;

    // ────────────── Assembly lifecycle ──────────────

    /// <summary>Starts the Kestrel test server and chromedriver process once for the whole assembly.</summary>
    [AssemblyInitialize]
    public static async Task AssemblyInit(TestContext _)
    {
        _server = new TestWebServer();
        await _server.StartAsync();
        BaseUrl = _server.BaseUrl;

        // Warm up the server: the first HTTP request triggers JIT, middleware
        // initialization, and Razor view compilation.  Doing this here (once)
        // prevents the much tighter WebDriverWait timeouts from being exceeded
        // when Chrome navigates for the first time.
        using var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };
        using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(60) };
        await client.GetAsync(BaseUrl);

        _driverService = ChromeDriverService.CreateDefaultService();
        _driverService.SuppressInitialDiagnosticInformation = true;
        _driverService.HideCommandPromptWindow = true;
        _driverService.Start();
    }

    /// <summary>Stops chromedriver and the Kestrel test server.</summary>
    [AssemblyCleanup]
    public static async Task AssemblyCleanup()
    {
        _driverService?.Dispose();
        if (_server is not null)
            await _server.DisposeAsync();
    }

    // ────────────── Class lifecycle (one Chrome browser per class) ──────────────

    [ClassInitialize(InheritanceBehavior.BeforeEachDerivedClass)]
    public static void BaseClassInit(TestContext _)
    {
        ChromeOptions options = CreateOptimizedChromeOptions();
        _driver = new RemoteWebDriver(_driverService.ServiceUrl, options.ToCapabilities(), TimeSpan.FromSeconds(30));
        _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);
        _driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(20);
        _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10))
        {
            PollingInterval = TimeSpan.FromMilliseconds(250)
        };
        _isLoggedIn = false;

        // Place the browser on a valid HTTP domain so that cookie
        // operations (e.g. ClearSession → DeleteAllCookies) work
        // on the very first test.  A fresh Chrome sits on about:blank
        // which has no domain context and causes DeleteAllCookies to fail.
        _driver.Navigate().GoToUrl(BaseUrl);
    }

    [ClassCleanup(InheritanceBehavior.BeforeEachDerivedClass)]
    public static void BaseClassCleanup()
    {
        try { _driver?.Quit(); } catch { /* session may already be dead */ }
        _driver = null;
        _wait = null;
        _isLoggedIn = false;
    }

    // ────────────── Chrome options tuned for speed ──────────────

    private static ChromeOptions CreateOptimizedChromeOptions()
    {
        ChromeOptions options = new()
        {
            PageLoadStrategy = PageLoadStrategy.Eager   // Don't wait for images / sub-resources
        };

        options.AddArgument("--headless=new");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-gpu");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--ignore-certificate-errors");
        options.AddArgument("--window-size=1920,1080");

        // Disable non-essential features for faster page loads.
        options.AddArgument("--disable-extensions");
        options.AddArgument("--disable-infobars");
        options.AddArgument("--disable-notifications");
        options.AddArgument("--disable-default-apps");
        options.AddArgument("--disable-translate");
        options.AddArgument("--disable-background-networking");
        options.AddArgument("--disable-sync");
        options.AddArgument("--no-first-run");
        options.AddArgument("--dns-prefetch-disable");
        options.AddArgument("--blink-settings=imagesEnabled=false");

        return options;
    }

    // ─────────────── Login / Logout helpers ───────────────

    /// <summary>
    /// Logs in once per class. Subsequent calls within the same class are no-ops
    /// because the auth cookie persists in the shared browser session.
    /// If the Chrome session has died (e.g. crash under parallel load), the driver
    /// is transparently recreated so the test can proceed.
    /// </summary>
    protected void EnsureLoggedIn()
    {
        if (!IsDriverSessionValid())
        {
            RecreateDriver();
            _isLoggedIn = false;
        }

        if (!_isLoggedIn)
        {
            LoginAsSuperAdmin();
            _isLoggedIn = true;
        }
    }

    protected void LoginAsSuperAdmin()
    {
        Login("super@admin.com", "123456");
    }

    protected void Login(string email, string password)
    {
        Driver.Navigate().GoToUrl($"{BaseUrl}/Account/Login");

        Wait.Until(d => d.FindElement(By.Id("Email")));

        // Use JavaScript to set field values — Selenium's SendKeys() can silently
        // fail when navigating between two URLs that share the same path (e.g.
        // /Account/Login?ReturnUrl=... → /Account/Login) because the element
        // reference may briefly point to the outgoing page's DOM.
        IJavaScriptExecutor js = (IJavaScriptExecutor)Driver;
        js.ExecuteScript($"document.getElementById('Email').value = '{email}';");
        js.ExecuteScript($"document.getElementById('Password').value = '{password}';");
        js.ExecuteScript(
            $"document.getElementById('loginCaptchaToken').value = '{TestCaptchaService.TestToken}';");

        js.ExecuteScript("document.getElementById('loginForm').submit();");

        Wait.Until(d => !d.Url.Contains("/Account/Login"));
    }

    /// <summary>Clears all cookies so the next request is unauthenticated.</summary>
    protected void ClearSession()
    {
        try
        {
            Driver.Manage().Cookies.DeleteAllCookies();
        }
        catch (WebDriverException)
        {
            // The browser may be on about:blank (e.g. after a session recovery)
            // where there is no domain context for cookie operations.
            Driver.Navigate().GoToUrl(BaseUrl);
            Driver.Manage().Cookies.DeleteAllCookies();
        }

        _isLoggedIn = false;
    }

    protected void Logout()
    {
        ((IJavaScriptExecutor)Driver).ExecuteScript(
            "document.getElementById('logoutForm').submit();");

        Wait.Until(d =>
            d.Url.Contains("/Account/Login") ||
            d.Url.Contains("/UserRole/UserProfile"));
        _isLoggedIn = false;
    }

    // ─────────────── Session health & recovery ───────────────

    /// <summary>
    /// Checks whether the current WebDriver session is still valid.
    /// A session may become invalid if Chrome crashes under parallel execution load.
    /// </summary>
    private static bool IsDriverSessionValid()
    {
        if (_driver is null) return false;
        try
        {
            _ = _driver.Title; // lightweight command to probe the session
            return true;
        }
        catch (WebDriverException)
        {
            return false;
        }
    }

    /// <summary>
    /// Tears down the dead driver (if any) and creates a fresh Chrome session
    /// reusing the assembly-level <see cref="_driverService"/>.
    /// </summary>
    private static void RecreateDriver()
    {
        try { _driver?.Quit(); } catch { /* session already dead */ }

        ChromeOptions options = CreateOptimizedChromeOptions();
        _driver = new RemoteWebDriver(_driverService.ServiceUrl, options.ToCapabilities(), TimeSpan.FromSeconds(30));
        _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);
        _driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(20);
        _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10))
        {
            PollingInterval = TimeSpan.FromMilliseconds(250)
        };
    }

    // ─────────────── Navigation helpers ───────────────

    protected void NavigateTo(string relativePath)
    {
        try
        {
            Driver.Navigate().GoToUrl($"{BaseUrl}{relativePath}");
        }
        catch (WebDriverException) when (!IsDriverSessionValid())
        {
            // Chrome session died (e.g. crash under parallel load) — recover and retry.
            RecreateDriver();
            _isLoggedIn = false;
            EnsureLoggedIn();
            Driver.Navigate().GoToUrl($"{BaseUrl}{relativePath}");
        }
    }

    protected void WaitForGrid()
    {
        Wait.Until(d =>
        {
            var grids = d.FindElements(By.CssSelector(".e-grid"));
            if (grids.Count == 0) return false;
            // Wait until the grid finishes loading data (loading popup dismissed)
            return !grids[0].GetAttribute("class").Contains("e-waitingpopup");
        });
    }

    // ─────────────── Login-page assertion helpers ───────────────

    protected void WaitForLoginPage()
    {
        Wait.Until(d => d.Url.Contains("/Account/Login"));
    }

    protected void AssertRedirectedToLoginFor(string originalPath)
    {
        Wait.Until(d => d.Url.Contains("/Account/Login"));
        Assert.IsTrue(Driver.Url.Contains("/Account/Login"),
            $"Expected redirect to login when accessing {originalPath}. Actual URL: {Driver.Url}");
    }
}
