using coderush.Data;
using coderush.Models;
using coderush.Models.SyncfusionViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coderush.Controllers.Api
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/User")]
    public class UserController(ApplicationDbContext context,
                    UserManager<ApplicationUser> userManager) : Controller
    {
        private readonly ApplicationDbContext _context = context;
        private readonly UserManager<ApplicationUser> _userManager = userManager;

        // GET: api/User
        [HttpGet]
        public IActionResult GetUser()
        {
            List<UserProfile> Items = [.. _context.UserProfile];
            int Count = Items.Count;
            return Ok(new { Items, Count });
        }

        [HttpGet("[action]/{id}")]
        public IActionResult GetByApplicationUserId([FromRoute] string id)
        {
            UserProfile userProfile = _context.UserProfile.SingleOrDefault(x => x.ApplicationUserId.Equals(id));
            List<UserProfile> Items = [];
            if (userProfile != null)
            {
                Items.Add(userProfile);
            }
            int Count = Items.Count;
            return Ok(new { Items, Count });
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Insert([FromBody] CrudViewModel<UserProfile> payload)
        {
            UserProfile register = payload.value;
            if (register.Password.Equals(register.ConfirmPassword))
            {
                ApplicationUser user = new() { Email = register.Email, UserName = register.Email, EmailConfirmed = true };
                IdentityResult result = await _userManager.CreateAsync(user, register.Password);
                if (result.Succeeded)
                {
                    register.Password = user.PasswordHash;
                    register.ConfirmPassword = user.PasswordHash;
                    register.ApplicationUserId = user.Id;
                    _context.UserProfile.Add(register);
                    await _context.SaveChangesAsync();
                }

            }
            return Ok(register);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Update([FromBody] CrudViewModel<UserProfile> payload)
        {
            UserProfile profile = payload.value;
            _context.UserProfile.Update(profile);
            await _context.SaveChangesAsync();
            return Ok(profile);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ChangePassword([FromBody] CrudViewModel<UserProfile> payload)
        {
            UserProfile profile = payload.value;

            if (string.IsNullOrEmpty(profile.OldPassword))
            {
                return BadRequest("Old password is required.");
            }

            if (string.IsNullOrEmpty(profile.Password))
            {
                return BadRequest("New password is required.");
            }

            if (!profile.Password.Equals(profile.ConfirmPassword))
            {
                return BadRequest("New password and confirm password do not match.");
            }

            ApplicationUser user = await _userManager.FindByIdAsync(profile.ApplicationUserId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            IdentityResult result = await _userManager.ChangePasswordAsync(user, profile.OldPassword, profile.Password);
            if (!result.Succeeded)
            {
                string errors = string.Join(" ", result.Errors.Select(e => e.Description));
                return BadRequest(errors);
            }

            profile = _context.UserProfile.SingleOrDefault(x => x.ApplicationUserId.Equals(profile.ApplicationUserId));
            return Ok(profile);
        }

        [HttpPost("[action]")]
        public IActionResult ChangeRole([FromBody] CrudViewModel<UserProfile> payload)
        {
            UserProfile profile = payload.value;
            return Ok(profile);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Remove([FromBody] CrudViewModel<UserProfile> payload)
        {
            UserProfile userProfile = _context.UserProfile.SingleOrDefault(x => x.UserProfileId.Equals(Convert.ToInt32(payload.key)));
            if (userProfile != null)
            {
                ApplicationUser user = _context.Users.Where(x => x.Id.Equals(userProfile.ApplicationUserId)).FirstOrDefault();
                IdentityResult result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    _context.Remove(userProfile);
                    await _context.SaveChangesAsync();
                }

            }

            return Ok();

        }


    }
}