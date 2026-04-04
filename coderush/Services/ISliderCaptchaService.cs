namespace coderush.Services
{
    public interface ISliderCaptchaService
    {
        (string Token, int PuzzleX) GenerateChallenge();
        string Validate(string challengeToken, int userX, long solveTimeMs);
        bool IsVerified(string verificationToken);
    }
}
