using coderush.AutomatedTests.Infrastructure;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace coderush.AutomatedTests.Tests;

[TestClass]
public sealed class AuthenticationTests : SeleniumTestBase
{
    [TestInitialize]
    public void ClearStateBeforeEachTest() => ClearSession();

    [TestMethod]
    public void Login_ValidCredentials_RedirectsAwayFromLoginPage()
    {
        LoginAsSuperAdmin();

        Assert.IsFalse(Driver.Url.Contains("/Account/Login"),
            $"Expected to leave the login page but URL is: {Driver.Url}");
    }

    [TestMethod]
    public void Login_InvalidPassword_ShowsErrorMessage()
    {
        NavigateTo("/Account/Login");

        Wait.Until(d => d.FindElement(By.Id("Email")));

        IWebElement emailField = Driver.FindElement(By.Id("Email"));
        emailField.Clear();
        emailField.SendKeys("super@admin.com");

        IWebElement passwordField = Driver.FindElement(By.Id("Password"));
        passwordField.Clear();
        passwordField.SendKeys("WrongPassword!");

        ((IJavaScriptExecutor)Driver).ExecuteScript(
            $"document.getElementById('loginCaptchaToken').value = '{TestCaptchaService.TestToken}';");

        ((IJavaScriptExecutor)Driver).ExecuteScript("document.getElementById('loginForm').submit();");

        // Should stay on the login page with an error message.
        Wait.Until(d => d.PageSource.Contains("Invalid login attempt"));
        Assert.IsTrue(Driver.Url.Contains("/Account/Login"));
    }

    [TestMethod]
    public void ProtectedPage_Unauthenticated_RedirectsToLogin()
    {
        const string protectedPath = "/Customer/Index";

        // Navigate to a neutral page first so the subsequent request is a fresh round-trip
        // after cookies were cleared by ClearStateBeforeEachTest.
        NavigateTo("/Account/Login");
        WaitForLoginPage();

        NavigateTo(protectedPath);

        AssertRedirectedToLoginFor(protectedPath);
    }

    [TestMethod]
    public void Logout_AuthenticatedUser_RedirectsToLoginOrHome()
    {
        const string protectedPath = "/Customer/Index";

        // JS-based login: avoids slow character-by-character SendKeys.
        NavigateTo("/Account/Login");

        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.Zero;
        try
        {
            string loginScript = $"""
                var f = document.getElementById('loginForm');
                if (!f) return false;
                var e = document.getElementById('Email');
                var p = document.getElementById('Password');
                if (!e || !p) return false;
                e.value = 'super@admin.com';
                p.value = '123456';
                var t = document.getElementById('loginCaptchaToken');
                if (t) t.value = '{TestCaptchaService.TestToken}';
                f.submit();
                return true;
                """;

            Wait.Until(d =>
            {
                try
                {
                    object? result = ((IJavaScriptExecutor)d).ExecuteScript(loginScript);
                    return result is true;
                }
                catch (WebDriverException) { return false; }
            });

            Wait.Until(d =>
            {
                try { return !d.Url.Contains("/Account/Login"); }
                catch (WebDriverException) { return false; }
            });

            // Find and submit the logout form in a single JS poll.
            Wait.Until(d =>
            {
                try
                {
                    object? result = ((IJavaScriptExecutor)d).ExecuteScript(
                        "var f = document.getElementById('logoutForm'); if (f) { f.submit(); return true; } return false;");
                    return result is true;
                }
                catch (WebDriverException) { return false; }
            });

            // Wait for URL to change away from the pre-logout page.
            Wait.Until(d =>
            {
                try { return d.Url.Contains("/Account/Login", StringComparison.Ordinal) || d.Url.Contains("/Home", StringComparison.Ordinal); }
                catch (WebDriverException) { return false; }
            });

            // Verify: navigating to a protected page redirects to login.
            Driver.Navigate().GoToUrl($"{BaseUrl}{protectedPath}");

            Wait.Until(d =>
            {
                try { return d.Url.Contains("/Account/Login"); }
                catch (WebDriverException) { return false; }
            });

            Assert.IsTrue(Driver.Url.Contains("/Account/Login"),
                $"Expected redirect to login but URL is: {Driver.Url}");
            Assert.IsTrue(
                Driver.Url.Contains(Uri.EscapeDataString(protectedPath)) ||
                Driver.Url.Contains(protectedPath),
                $"Expected login redirect to preserve return URL for {protectedPath} but URL is: {Driver.Url}");
        }
        finally
        {
            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);
        }
    }

    [TestMethod]
    public void Register_NewUser_CreatesAccountAndRedirects()
    {
        string uniqueEmail = $"selenium_{Guid.NewGuid():N}@test.com";

        NavigateTo("/Account/Register");

        // Fill every field and submit in a single JS round-trip — replaces
        // WaitForRegisterPage + 3× SetInputValue + SetHiddenInputValue + SubmitForm.
        // Disable the 3 s implicit wait so any FindElements polling returns
        // instantly rather than blocking 3 s per miss.
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.Zero;
        try
        {
            string fillAndSubmitScript = $"""
                var f = document.getElementById('registerForm');
                if (!f) return false;
                var e = document.getElementById('Email');
                var p = document.getElementById('Password');
                var c = document.getElementById('ConfirmPassword');
                if (!e || !p || !c) return false;
                e.value = '{uniqueEmail}';
                p.value = 'Test123456';
                c.value = 'Test123456';
                var t = document.getElementById('registerCaptchaToken');
                if (t) t.value = '{TestCaptchaService.TestToken}';
                f.submit();
                return true;
                """;

            Wait.Until(d =>
            {
                try
                {
                    object? result = ((IJavaScriptExecutor)d).ExecuteScript(fillAndSubmitScript);
                    return result is true;
                }
                catch (WebDriverException) { return false; }
            });

            Wait.Until(d =>
            {
                try { return !d.Url.Contains("/Account/Register"); }
                catch (WebDriverException) { return false; }
            });
        }
        finally
        {
            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);
        }

        Assert.IsFalse(Driver.Url.Contains("/Account/Register"),
            "Expected to leave the register page after successful registration.");

        NavigateTo("/UserRole/UserProfile");

        // Merge WaitForAuthenticatedPage + email-presence check into one loop.
        Wait.Until(d =>
        {
            try
            {
                string url = d.Url;
                return url.Contains("/UserRole/UserProfile", StringComparison.Ordinal) &&
                       !url.Contains("/Account/Login", StringComparison.Ordinal) &&
                       d.PageSource.Contains(uniqueEmail, StringComparison.Ordinal);
            }
            catch (WebDriverException) { return false; }
        });

        Assert.IsFalse(Driver.Url.Contains("/Account/Login"),
            "Expected the newly registered user to be signed in automatically.");
        Assert.IsTrue(Driver.PageSource.Contains(uniqueEmail),
            $"Expected user profile page to contain the registered email '{uniqueEmail}'.");
    }

    private void WaitForLoginPage()
    {
        Wait.Until(d =>
        {
            try
            {
                return d.Url.Contains("/Account/Login") &&
                       d.FindElements(By.Id("Email")).Count > 0 &&
                       d.FindElements(By.Id("Password")).Count > 0;
            }
            catch (WebDriverException) { return false; }
        });
    }

    private void AssertRedirectedToLoginFor(string protectedPath)
    {
        WaitForLoginPage();

        Assert.IsTrue(Driver.Url.Contains("/Account/Login"),
            $"Expected redirect to login but URL is: {Driver.Url}");
        Assert.IsTrue(
            Driver.Url.Contains(Uri.EscapeDataString(protectedPath)) ||
            Driver.Url.Contains(protectedPath),
            $"Expected login redirect to preserve return URL for {protectedPath} but URL is: {Driver.Url}");
    }
}
