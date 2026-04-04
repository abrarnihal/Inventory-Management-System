using coderush.Services;

namespace coderush.IntegrationTests.Infrastructure;

/// <summary>
/// A test-only captcha service that always verifies successfully.
/// Injected by <see cref="CustomWebApplicationFactory"/> so that login
/// and register flows work without real slider-captcha interaction.
/// </summary>
public sealed class TestCaptchaService : ISliderCaptchaService
{
    public const string TestToken = "integration-test-captcha-token";

    public (string Token, int PuzzleX) GenerateChallenge() => (TestToken, 100);

    public string Validate(string challengeToken, int userX, long solveTimeMs) => TestToken;

    public bool IsVerified(string verificationToken) => true;
}
