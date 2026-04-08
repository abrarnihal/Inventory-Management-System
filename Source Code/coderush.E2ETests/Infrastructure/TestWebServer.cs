using coderush.Data;
using coderush.Models;
using coderush.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;

namespace coderush.E2ETests.Infrastructure;

/// <summary>
/// Starts a real Kestrel HTTP server hosting the full coderush application
/// with an EF Core InMemory database and a test captcha service.
/// Selenium WebDriver connects to the <see cref="BaseUrl"/> address.
/// </summary>
public sealed class TestWebServer : IAsyncDisposable
{
    private WebApplication? _app;

    /// <summary>The <c>https://127.0.0.1:{port}</c> address the server is listening on.</summary>
    public string BaseUrl { get; private set; } = "";

    public async Task StartAsync()
    {
        string contentRoot = FindCoderushProjectDirectory();

        WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ContentRootPath = contentRoot,
            WebRootPath = Path.Combine(contentRoot, "wwwroot"),
            EnvironmentName = "Development"
        });

        builder.WebHost.UseUrls("https://127.0.0.1:0");

        ConfigureServices(builder);

        _app = builder.Build();

        ConfigureMiddleware(_app);

        await SeedDatabaseAsync(_app);

        await _app.StartAsync();

        // Resolve the dynamically-assigned port.
        IServer server = _app.Services.GetRequiredService<IServer>();
        IServerAddressesFeature? addresses = server.Features.Get<IServerAddressesFeature>();
        BaseUrl = addresses!.Addresses.First();
    }

    // ─────────────── Service registration (mirrors Program.cs) ───────────────

    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        // InMemory database instead of SQL Server.
        string dbName = $"E2ETestDb_{Guid.NewGuid()}";
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase(dbName));

        // ASP.NET Identity with relaxed password rules (same as appsettings.json defaults).
        builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
            options.Password.RequiredUniqueChars = 0;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = false;
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = false;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            options.ExpireTimeSpan = TimeSpan.FromDays(150);
            options.LoginPath = "/Account/Login";
            options.LogoutPath = "/Account/Logout";
            options.AccessDeniedPath = "/Account/AccessDenied";
            options.SlidingExpiration = true;

            options.Events.OnRedirectToLogin = context =>
            {
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                }
                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            };
        });

        // Options objects (empty defaults suffice for tests).
        builder.Services.Configure<SendGridOptions>(_ => { });
        builder.Services.Configure<SmtpOptions>(_ => { });
        builder.Services.Configure<SuperAdminDefaultOptions>(o =>
        {
            o.Email = "super@admin.com";
            o.Password = "123456";
        });
        builder.Services.Configure<IdentityDefaultOptions>(_ => { });
        builder.Services.Configure<OpenAIOptions>(_ => { });

        // Application services.
        builder.Services.AddTransient<IEmailSender, EmailSender>();
        builder.Services.AddTransient<INumberSequence, coderush.Services.NumberSequence>();
        builder.Services.AddTransient<IRoles, Roles>();
        builder.Services.AddTransient<IFunctional, Functional>();

        builder.Services.AddHttpClient<IChatBotService, ChatBotService>();
        builder.Services.AddSingleton<IFileParserService, FileParserService>();

        // Test-only captcha that always validates.
        builder.Services.AddSingleton<ISliderCaptchaService, TestCaptchaService>();

        // MVC (same settings as production).
        builder.Services.AddMvc(options => options.EnableEndpointRouting = false)
            .AddApplicationPart(typeof(coderush.Controllers.AccountController).Assembly)
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling =
                    Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                options.SerializerSettings.ContractResolver =
                    new DefaultContractResolver();
            });
    }

    // ─────────────── Middleware pipeline (mirrors Program.cs) ───────────────

    private static void ConfigureMiddleware(WebApplication app)
    {
        app.UseDeveloperExceptionPage();
        app.UseStaticFiles();
        app.UseAuthentication();
        app.UseMvc(routes =>
        {
            routes.MapRoute(
                name: "default",
                template: "{controller=UserRole}/{action=UserProfile}/{id?}");
        });
    }

    // ─────────────── Database seeding ───────────────

    private static async Task SeedDatabaseAsync(WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        IFunctional functional = scope.ServiceProvider.GetRequiredService<IFunctional>();
        await DbInitializer.Initialize(context, functional);
    }

    // ─────────────── Helpers ───────────────

    private static string FindCoderushProjectDirectory()
    {
        string? dir = AppContext.BaseDirectory;
        while (dir is not null)
        {
            string candidate = Path.Combine(dir, "coderush");
            if (Directory.Exists(candidate) && File.Exists(Path.Combine(candidate, "coderush.csproj")))
                return candidate;

            if (Directory.GetFiles(dir, "*.sln").Length > 0)
            {
                candidate = Path.Combine(dir, "coderush");
                if (Directory.Exists(candidate))
                    return candidate;
            }

            dir = Directory.GetParent(dir)?.FullName;
        }

        throw new InvalidOperationException(
            "Could not locate the 'coderush' project directory. " +
            "Ensure the test is run from within the solution tree.");
    }

    public async ValueTask DisposeAsync()
    {
        if (_app is not null)
        {
            await _app.StopAsync();
            await _app.DisposeAsync();
        }
    }
}
