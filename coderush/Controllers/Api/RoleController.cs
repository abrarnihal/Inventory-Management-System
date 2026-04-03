using coderush.Models;
using coderush.Models.AccountViewModels;
using coderush.Models.SyncfusionViewModels;
using coderush.Services;
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
    [Route("api/Role")]
    public class RoleController(UserManager<ApplicationUser> userManager,
                    RoleManager<IdentityRole> roleManager,
                    IRoles roles) : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly IRoles _roles = roles;

        // GET: api/Role
        [HttpGet]
        public async Task<IActionResult> GetRole()
        {
            await _roles.GenerateRolesFromPagesAsync();

            List<IdentityRole> Items = [.. _roleManager.Roles];
            int Count = Items.Count;
            return Ok(new { Items, Count });
        }

        // GET: api/Role
        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> GetRoleByApplicationUserId([FromRoute] string id)
        {
            await _roles.GenerateRolesFromPagesAsync();
            ApplicationUser user = await _userManager.FindByIdAsync(id);
            var roles = _roleManager.Roles.ToList();
            List<UserRoleViewModel> Items = [];
            int count = 1;
            foreach (IdentityRole item in roles)
            {
                bool isInRole = await _userManager.IsInRoleAsync(user, item.Name);
                Items.Add(new() { CounterId = count, ApplicationUserId = id, RoleName = item.Name, IsHaveAccess = isInRole });
                count++;
            }

            int Count = Items.Count;
            return Ok(new { Items, Count });
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> UpdateUserRole([FromBody] CrudViewModel<UserRoleViewModel> payload)
        {
            UserRoleViewModel userRole = payload.value;
            if (userRole != null)
            {
                ApplicationUser user = await _userManager.FindByIdAsync(userRole.ApplicationUserId);
                if (user != null)
                {
                    if (userRole.IsHaveAccess)
                    {
                        await _userManager.AddToRoleAsync(user, userRole.RoleName);
                    }
                    else
                    {
                        await _userManager.RemoveFromRoleAsync(user, userRole.RoleName);
                    }
                }
            }
            return Ok(userRole);
        }
    }
}