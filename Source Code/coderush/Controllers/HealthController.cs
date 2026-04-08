using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace coderush.Controllers
{
    [AllowAnonymous]
    [Route("[controller]")]
    public class HealthController : Controller
    {
        [HttpGet("/healthz")]
        public IActionResult Index()
        {
            return Ok("Healthy");
        }
    }
}
