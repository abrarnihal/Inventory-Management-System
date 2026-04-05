using coderush.Data;
using coderush.Models;
using coderush.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using System.Threading.Tasks;

namespace coderush
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Load .env file so secrets stay out of appsettings.json
            LoadEnvFile(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".env"));
            LoadEnvFile(Path.Combine(Directory.GetCurrentDirectory(), ".env"));

            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            
            builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();

            // Determine the database connection string to use.
            // In Development, try the Azure connection first; if it is unreachable,
            // fall back to the LocalDB connection so developers can work offline.
            string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            if (builder.Environment.IsDevelopment())
            {
                string localConnection = builder.Configuration.GetConnectionString("LocalConnection");
                if (!string.IsNullOrEmpty(localConnection))
                {
                    try
                    {
                        using var testConn = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
                        testConn.Open();
                    }
                    catch
                    {
                        Console.WriteLine("⚠ Azure SQL database is unavailable – falling back to LocalDB.");
                        connectionString = localConnection;
                    }
                }
            }

            // Configure services
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Get Identity Default Options
            IConfigurationSection identityDefaultOptionsConfigurationSection = builder.Configuration.GetSection("IdentityDefaultOptions");

            builder.Services.Configure<IdentityDefaultOptions>(identityDefaultOptionsConfigurationSection);

            IdentityDefaultOptions identityDefaultOptions = identityDefaultOptionsConfigurationSection.Get<IdentityDefaultOptions>();

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Password settings
                options.Password.RequireDigit = identityDefaultOptions.PasswordRequireDigit;
                options.Password.RequiredLength = identityDefaultOptions.PasswordRequiredLength;
                options.Password.RequireNonAlphanumeric = identityDefaultOptions.PasswordRequireNonAlphanumeric;
                options.Password.RequireUppercase = identityDefaultOptions.PasswordRequireUppercase;
                options.Password.RequireLowercase = identityDefaultOptions.PasswordRequireLowercase;
                options.Password.RequiredUniqueChars = identityDefaultOptions.PasswordRequiredUniqueChars;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(identityDefaultOptions.LockoutDefaultLockoutTimeSpanInMinutes);
                options.Lockout.MaxFailedAccessAttempts = identityDefaultOptions.LockoutMaxFailedAccessAttempts;
                options.Lockout.AllowedForNewUsers = identityDefaultOptions.LockoutAllowedForNewUsers;

                // User settings
                options.User.RequireUniqueEmail = identityDefaultOptions.UserRequireUniqueEmail;

                // email confirmation require
                options.SignIn.RequireConfirmedEmail = identityDefaultOptions.SignInRequireConfirmedEmail;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // cookie settings
            builder.Services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.Cookie.HttpOnly = identityDefaultOptions.CookieHttpOnly;
                options.ExpireTimeSpan = TimeSpan.FromDays(identityDefaultOptions.CookieExpiration);
                options.LoginPath = identityDefaultOptions.LoginPath;
                options.LogoutPath = identityDefaultOptions.LogoutPath;
                options.AccessDeniedPath = identityDefaultOptions.AccessDeniedPath;
                options.SlidingExpiration = identityDefaultOptions.SlidingExpiration;

                // Return 401 for unauthenticated API requests instead of redirecting to login
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

            // Get SendGrid configuration options
            builder.Services.Configure<SendGridOptions>(builder.Configuration.GetSection("SendGridOptions"));

            // Get SMTP configuration options
            builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("SmtpOptions"));

            // Get Super Admin Default options
            builder.Services.Configure<SuperAdminDefaultOptions>(builder.Configuration.GetSection("SuperAdminDefaultOptions"));

            // Add email services.
            builder.Services.AddTransient<IEmailSender, EmailSender>();

            builder.Services.AddTransient<INumberSequence, Services.NumberSequence>();

            builder.Services.AddTransient<IRoles, Roles>();

            builder.Services.AddTransient<IFunctional, Functional>();

            builder.Services.Configure<OpenAIOptions>(builder.Configuration.GetSection("OpenAI"));
            builder.Services.AddHttpClient<IChatBotService, ChatBotService>();
            builder.Services.AddSingleton<IChatResponseOrchestrator, ChatResponseOrchestrator>();
            builder.Services.AddSingleton<IFileParserService, FileParserService>();

            builder.Services.AddSingleton<ISliderCaptchaService, SliderCaptchaService>();

            builder.Services.AddMvc(options =>
            {
                options.EnableEndpointRouting = false;
            })
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
            });

            WebApplication app = builder.Build();

            // Configure middleware pipeline

            // Handle forwarded headers from Azure App Service reverse proxy (ARR).
            // Without this, the app sees HTTP instead of HTTPS behind the load balancer,
            // causing auth cookie and anti-forgery token failures.
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=UserRole}/{action=UserProfile}/{id?}");
            });

            using (IServiceScope scope = app.Services.CreateScope())
            {
                IServiceProvider services = scope.ServiceProvider;
                try
                {
                    ApplicationDbContext context = services.GetRequiredService<ApplicationDbContext>();
                    UserManager<ApplicationUser> userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                    RoleManager<IdentityRole> roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                    IFunctional functional = services.GetRequiredService<IFunctional>();

                    // Ensure ChatConversation.Title column exists (idempotent)
                    try
                    {
                        await context.Database.ExecuteSqlRawAsync(@"
                            IF COL_LENGTH('ChatConversation', 'Title') IS NULL
                            BEGIN
                                ALTER TABLE ChatConversation ADD Title nvarchar(max) NULL;
                            END");
                    }
                    catch
                    {
                        // Table may not exist yet on a fresh database
                    }

                    // Ensure ChatConversation.IsPinned column exists (idempotent)
                    try
                    {
                        await context.Database.ExecuteSqlRawAsync(@"
                            IF COL_LENGTH('ChatConversation', 'IsPinned') IS NULL
                            BEGIN
                                ALTER TABLE ChatConversation ADD IsPinned bit NOT NULL DEFAULT 0;
                            END");
                    }
                    catch
                    {
                        // Table may not exist yet on a fresh database
                    }

                    // Ensure PurchaseOrder.Description column exists (idempotent)
                    try
                    {
                        await context.Database.ExecuteSqlRawAsync(@"
                            IF COL_LENGTH('PurchaseOrder', 'Description') IS NULL
                            BEGIN
                                ALTER TABLE PurchaseOrder ADD Description nvarchar(max) NULL;
                            END");
                    }
                    catch
                    {
                        // Table may not exist yet on a fresh database
                    }

                    // Ensure Bill.Description column exists (idempotent)
                    try
                    {
                        await context.Database.ExecuteSqlRawAsync(@"
                            IF COL_LENGTH('Bill', 'Description') IS NULL
                            BEGIN
                                ALTER TABLE Bill ADD Description nvarchar(max) NULL;
                            END");
                    }
                    catch
                    {
                        // Table may not exist yet on a fresh database
                    }

                    // Ensure Invoice.Description column exists (idempotent)
                    try
                    {
                        await context.Database.ExecuteSqlRawAsync(@"
                            IF COL_LENGTH('Invoice', 'Description') IS NULL
                            BEGIN
                                ALTER TABLE Invoice ADD Description nvarchar(max) NULL;
                            END");
                    }
                    catch
                    {
                        // Table may not exist yet on a fresh database
                    }

                    await DbInitializer.Initialize(context, functional);
                }
                catch (Exception ex)
                {
                    ILogger<Program> logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while seeding the database.");
                }
            }

            app.Run();
        }

        private static void LoadEnvFile(string path)
        {
            if (!File.Exists(path))
                return;

            foreach (string line in File.ReadAllLines(path))
            {
                string trimmed = line.Trim();
                if (trimmed.Length == 0 || trimmed.StartsWith('#'))
                    continue;

                int index = trimmed.IndexOf('=');
                if (index <= 0)
                    continue;

                string key = trimmed[..index].Trim();
                string value = trimmed[(index + 1)..].Trim();
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }
}