using coderush.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace coderush.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => RedirectToAction("UserProfile", "UserRole");

        public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
