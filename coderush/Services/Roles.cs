using coderush.Models;
using coderush.Pages;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace coderush.Services
{
    public class Roles(RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager) : IRoles
    {
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly UserManager<ApplicationUser> _userManager = userManager;

        public async Task GenerateRolesFromPagesAsync()
        {
            Type t = typeof(MainMenu);
            foreach (Type item in t.GetNestedTypes())
            {
                foreach (FieldInfo itm in item.GetFields())
                {
                    if (itm.Name.Contains("RoleName"))
                    {
                        string roleName = (string)itm.GetValue(item);
                        if (!await _roleManager.RoleExistsAsync(roleName))
                            await _roleManager.CreateAsync(new IdentityRole(roleName));
                    }
                }
            }
        }

        public async Task AddToRoles(string applicationUserId)
        {
            ApplicationUser user = await _userManager.FindByIdAsync(applicationUserId);
            if (user != null)
            {
                IQueryable<IdentityRole> roles = _roleManager.Roles;
                List<string> listRoles = [];
                foreach (IdentityRole item in roles)
                {
                    listRoles.Add(item.Name);
                }
                await _userManager.AddToRolesAsync(user, listRoles);
            }
        }
    }
}
