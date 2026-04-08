using coderush.Data;
using coderush.Models;
using coderush.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coderush.Controllers.Api
{
    [Produces("application/json")]
    [Route("api/UploadProfilePicture")]
    [Authorize]
    public class UploadProfilePictureController(IFunctional functionalService,
        IWebHostEnvironment env,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context) : Controller
    {
        private readonly IFunctional _functionalService = functionalService;
        private readonly IWebHostEnvironment _env = env;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly ApplicationDbContext _context = context;

        private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp"];

        [HttpPost]
        [RequestSizeLimit(5000000)]
        public async Task<IActionResult> PostUploadProfilePicture(List<IFormFile> UploadDefault)
        {
            try
            {
                foreach (var file in UploadDefault)
                {
                    var extension = System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();
                    if (string.IsNullOrEmpty(extension) || !AllowedImageExtensions.Contains(extension))
                    {
                        return BadRequest(new { message = "Only image files (.jpg, .jpeg, .png, .gif, .bmp, .webp) are allowed." });
                    }

                    if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                    {
                        return BadRequest(new { message = "Only image files are allowed." });
                    }
                }

                var folderUpload = "upload";
                var fileName = await _functionalService.UploadFile(UploadDefault, _env, folderUpload);

                ApplicationUser appUser = await _userManager.GetUserAsync(User);
                if (appUser != null)
                {
                    UserProfile profile = _context.UserProfile.SingleOrDefault(x => x.ApplicationUserId.Equals(appUser.Id));
                    if (profile != null)
                    {
                        profile.ProfilePicture = "/" + folderUpload + "/" + fileName;
                        _context.UserProfile.Update(profile);
                        await _context.SaveChangesAsync();
                    }
                }
                return Ok(fileName);
            }
            catch (Exception ex)
            {

                return StatusCode(500, new { message = ex.Message });
            }


        }
    }
}