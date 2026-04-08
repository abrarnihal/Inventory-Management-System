using coderush.Data;
using coderush.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace coderush.IntegrationTests.Infrastructure;

/// <summary>
/// A <see cref="WebApplicationFactory{TEntryPoint}"/> that replaces the
/// SQL Server database with an EF Core InMemory database and swaps the
/// real slider-captcha service with <see cref="TestCaptchaService"/>.
/// The factory seeds the same default data that the production app seeds
/// via <see cref="DbInitializer"/> so that every test class starts with
/// a known baseline (reference types, sample products, customers, vendors,
/// and a super-admin user).
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            // Remove ALL DbContext-related registrations (options, context, and
            // internal configuration objects such as DbContextOptionsConfiguration)
            // so that the InMemory provider doesn't clash with the SqlServer provider.
            var dbDescriptors = services
                .Where(d => d.ServiceType == typeof(ApplicationDbContext)
                         || (d.ServiceType.IsGenericType
                             && d.ServiceType.GetGenericArguments().Contains(typeof(ApplicationDbContext))))
                .ToList();
            foreach (ServiceDescriptor d in dbDescriptors)
                services.Remove(d);

            // Add an InMemory database for testing.
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));

            // Replace captcha service with always-valid test double.
            ServiceDescriptor? captchaDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(ISliderCaptchaService));
            if (captchaDescriptor is not null)
                services.Remove(captchaDescriptor);

            services.AddSingleton<ISliderCaptchaService, TestCaptchaService>();
        });
    }
}
