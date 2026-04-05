using coderush.Services;

namespace coderush.AutomatedTests.Infrastructure;

/// <summary>
/// A captcha service that always validates successfully so that Selenium
/// tests can submit the login and register forms without real slider interaction.
/// The test server injects a valid token into the hidden captcha field via JavaScript.
/// </summary>
public sealed class TestCaptchaService : ISliderCaptchaService
{
    public const string TestToken = "selenium-test-captcha-token";

    public (string Token, int PuzzleX) GenerateChallenge() => (TestToken, 100);

    public string Validate(string challengeToken, int userX, long solveTimeMs) => TestToken;

    public bool IsVerified(string verificationToken) => true;
}
