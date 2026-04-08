using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace coderush.E2ETests.Infrastructure.PageObjects;

/// <summary>Page Object for the /Account/Login page.</summary>
public class LoginPage
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;
    private readonly string _baseUrl;

    public LoginPage(IWebDriver driver, WebDriverWait wait, string baseUrl)
    {
        _driver = driver;
        _wait = wait;
        _baseUrl = baseUrl;
    }

    public void Navigate() => _driver.Navigate().GoToUrl($"{_baseUrl}/Account/Login");

    public void LoginAs(string email, string password)
    {
        Navigate();
        _wait.Until(d => d.FindElement(By.Id("Email")));

        IWebElement emailField = _driver.FindElement(By.Id("Email"));
        emailField.Clear();
        emailField.SendKeys(email);

        IWebElement passwordField = _driver.FindElement(By.Id("Password"));
        passwordField.Clear();
        passwordField.SendKeys(password);

        ((IJavaScriptExecutor)_driver).ExecuteScript(
            $"document.getElementById('loginCaptchaToken').value = '{TestCaptchaService.TestToken}';");

        ((IJavaScriptExecutor)_driver).ExecuteScript(
            "document.getElementById('loginForm').submit();");

        _wait.Until(d => !d.Url.Contains("/Account/Login"));
    }

    public void LoginAsSuperAdmin() => LoginAs("super@admin.com", "123456");

    public bool IsOnLoginPage => _driver.Url.Contains("/Account/Login");
}
