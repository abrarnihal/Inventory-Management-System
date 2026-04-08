using coderush.Services;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace coderush.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(ApplicationDbContext context,
           IFunctional functional)
        {
            await context.Database.EnsureCreatedAsync();

            // check for users
            if (await context.ApplicationUser.AnyAsync())
            {
                return; // if user is not empty, DB has been seed
            }

            //init app with super admin user
            await functional.CreateDefaultSuperAdmin();

            //init app data
            await functional.InitAppData();

        }
    }
}
