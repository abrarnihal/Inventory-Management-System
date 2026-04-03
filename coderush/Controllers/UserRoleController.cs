using coderush.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace coderush.Controllers
{

    public class UserRoleController(UserManager<ApplicationUser> userManager) : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;

        [Authorize(Roles = Pages.MainMenu.User.RoleName)]
        public IActionResult Index() => View();

        [Authorize(Roles = Pages.MainMenu.ChangePassword.RoleName)]
        public IActionResult ChangePassword() => View();

        [Authorize(Roles = Pages.MainMenu.Role.RoleName)]
        public IActionResult Role() => View();

        [Authorize(Roles = Pages.MainMenu.ChangeRole.RoleName)]
        public IActionResult ChangeRole() => View();

        [Authorize]
        public async Task<IActionResult> UserProfile()
        {
            ApplicationUser user = await _userManager.GetUserAsync(User);
            return View(user);
        }
    }
}