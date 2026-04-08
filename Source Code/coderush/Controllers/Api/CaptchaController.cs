using coderush.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace coderush.Controllers.Api
{
    [AllowAnonymous]
    [Produces("application/json")]
    [Route("api/Captcha")]
    public class CaptchaController(ISliderCaptchaService captchaService) : Controller
    {
        private readonly ISliderCaptchaService _captchaService = captchaService;

        [HttpGet("Challenge")]
        public IActionResult GetChallenge()
        {
            var (token, puzzleX) = _captchaService.GenerateChallenge();
            return Ok(new { Token = token, PuzzleX = puzzleX });
        }

        [HttpPost("Verify")]
        public IActionResult Verify([FromBody] CaptchaVerifyRequest request)
        {
            if (request == null)
                return Ok(new { Success = false });

            string verificationToken = _captchaService.Validate(
                request.Token, request.UserX, request.SolveTimeMs);

            if (verificationToken is null)
                return Ok(new { Success = false });

            return Ok(new { Success = true, VerificationToken = verificationToken });
        }
    }

    public class CaptchaVerifyRequest
    {
        public string Token { get; set; } = "";
        public int UserX { get; set; }
        public long SolveTimeMs { get; set; }
    }
}
