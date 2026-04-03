using coderush.Data;
using coderush.Models;
using coderush.Models.ManageViewModels;
using coderush.Models.SyncfusionViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coderush.Controllers.Api
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/ChangePassword")]
    public class ChangePasswordController(ApplicationDbContext context,
                    UserManager<ApplicationUser> userManager) : Controller
    {
        private readonly ApplicationDbContext _context = context;
        private readonly UserManager<ApplicationUser> _userManager = userManager;

        // GET: api/ChangePassword
        [HttpGet]
        public IActionResult GetChangePassword()
        {
            List<ApplicationUser> Items = [.. _context.Users];
            int Count = Items.Count;
            return Ok(new { Items, Count });
        }



        [HttpPost("[action]")]
        public async Task<IActionResult> Update([FromBody] CrudViewModel<ChangePasswordViewModel> payload)
        {
            ChangePasswordViewModel changePasswordViewModel = payload.value;

            if (string.IsNullOrEmpty(changePasswordViewModel.OldPassword))
            {
                return BadRequest("Old password is required.");
            }

            if (string.IsNullOrEmpty(changePasswordViewModel.NewPassword))
            {
                return BadRequest("New password is required.");
            }

            if (!changePasswordViewModel.NewPassword.Equals(changePasswordViewModel.ConfirmPassword))
            {
                return BadRequest("New password and confirm password do not match.");
            }

            ApplicationUser user = _context.Users.SingleOrDefault(x => x.Id.Equals(changePasswordViewModel.Id));
            if (user == null)
            {
                return NotFound("User not found.");
            }

            IdentityResult result = await _userManager.ChangePasswordAsync(user, changePasswordViewModel.OldPassword, changePasswordViewModel.NewPassword);
            if (!result.Succeeded)
            {
                string errors = string.Join(" ", result.Errors.Select(e => e.Description));
                return BadRequest(errors);
            }

            return Ok();
        }

    }
}