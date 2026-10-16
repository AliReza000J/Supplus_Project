using _0_Framework.Application;
using _0_Framework.Infrastructure;
using _01_PharmaPlusQuery.Contracts.AdminNotification;
using _01_PharmaPlusQuery.Contracts.Dashbord;
using _01_PharmaPlusQuery.Query;
using AccountManagement.Configuration;
using AccountManagement.Domain.RoleAgg;
using AccountManagement.Infrastructure.EFCore;
using AspNetCoreRateLimit;
using BlogManagement.Configuration;
using CommentManagement.Infrastructure.Configuration;
using DiscountManagement.Configuration;
using Hangfire;
using InventoryManagement.Configuration;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Features;
using PharmaAdmin.Web.Filters;
using PharmaAdmin.Web.Middleware;
using PharmaAdmin.Web.Services;
using PharmaShared.Web;
using PharmaShared.Web.Middlewares;
using PharmaShared.Web.Options;
using PharmaShared.Web.Services;
using PharmaShared.Web.Services.ImageCleanup;
using Polly;
using QuestPDF.Infrastructure;
using Serilog;
using Serilog.Events;
using ShopManagement.Configuration;
using SiteManagement.Configuration;
using System.Text.Encodings.Web;
using System.Text.Unicode;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration;

var forceHttps = config.GetValue<bool>("Security:ForceHttps");

var origins =
    config.GetSection("Cors:Origins")
          .Get<string[]>() ?? [];

var connectionString =
    config.GetConnectionString("PharmaPlusDB")
    //config.GetConnectionString("Supplus_DB")
    ?? throw new InvalidOperationException(
        "Connection string 'PharmaPlusDB' was not found.");

// --- 1. Logging (Serilog) ---
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Filter.ByExcluding(le => le.MessageTemplate.Text.Contains("Authorization"))
    .WriteTo.Console()
    .WriteTo.File("Logs/app/app-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 90, restrictedToMinimumLevel: LogEventLevel.Information, shared: true)
    .WriteTo.File("Logs/errors/error-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 90, restrictedToMinimumLevel: LogEventLevel.Error, shared: true)
    .CreateLogger();

builder.Host.UseSerilog();

// --- 2. HTTP Clients & Base Services ---
builder.Services.AddHttpClient("SmsIr", client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
})
.AddTransientHttpErrorPolicy(policy =>
    policy.WaitAndRetryAsync(3, retry => TimeSpan.FromSeconds(2)))
.AddTransientHttpErrorPolicy(policy =>
    policy.CircuitBreakerAsync(
        handledEventsAllowedBeforeBreaking: 5,
        durationOfBreak: TimeSpan.FromMinutes(2)));

builder.Services.AddHttpContextAccessor();

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 5 * 1024 * 1024; // 5MB
});

// --- 3. Bootstrappers (IoC) ---
ShopManagementBootstrapper.Configure(builder.Services, connectionString);
SiteManagementBootstrapper.Configure(builder.Services, connectionString);
DiscountManagementBootstrapper.Configure(builder.Services, connectionString);
InventoryManagementBootstrapper.Configure(builder.Services, connectionString);
BlogManagementBootstrapper.Configure(builder.Services, connectionString);
CommentManagementBootstrapper.Configure(builder.Services, connectionString);
AccountManagementBootstrapper.Configure(builder.Services, connectionString);

// --- 4. Caching & Hangfire & PDF & IPRateLimiter ---
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ICacheService, MemoryCacheService>();

builder.Services.AddHangfire(c =>
    c.UseSqlServerStorage(connectionString));
builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = 2;
});

QuestPDF.Settings.License = LicenseType.Community;

builder.Services.Configure<IpRateLimitOptions>(config.GetSection("IpRateLimiting"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// --- 5. Application Services ---
builder.Services.AddTransient<IFileUploader, FileUploader>();
builder.Services.AddTransient<IDashboardQuery, DashboardQuery>();
builder.Services.AddTransient<IAdminNotificationQuery, AdminNotificationQuery>();
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IAuthHelper, AuthHelper>();
builder.Services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Arabic));

//builder.Services.AddSingleton<IOtpRateLimiter, OtpRateLimiter>();
builder.Services.AddSingleton<IAdminSmsRateLimiter, AdminSmsRateLimiter>();

builder.Services.AddTransient<IZibalService, ZibalService>();
builder.Services.AddTransient<IJwtService, JwtService>();
builder.Services.AddTransient<IEmailService, EmailService>();

builder.Services.AddScoped<ISmsService, SmsService>();
builder.Services.AddScoped<PostTrackingSmsJob>();

// Background tasks
builder.Services.AddScoped<ITokenCleanupService, TokenCleanupService>();
builder.Services.AddHostedService<TokenCleanupBackgroundService>();
builder.Services.AddScoped<IModuleImageCleanupService, BlogImageCleanupService>();
builder.Services.AddScoped<IModuleImageCleanupService, ShopImageCleanupService>();
builder.Services.AddScoped<IModuleImageCleanupService, AccountImageCleanupService>();
builder.Services.AddHostedService<OrphanedImagesBackgroundService>();

// --- 6. Auth (Cookie Only) ---
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/Admin/Login";
    options.LogoutPath = "/Admin/Logout";
    options.AccessDeniedPath = "/Admin/AccessDenied";

    options.Cookie.Name = "PharmaPlusShopAuth";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;

    options.ExpireTimeSpan = TimeSpan.FromDays(3);
    options.SlidingExpiration = true;

    options.Cookie.SecurePolicy =
        forceHttps
            ? CookieSecurePolicy.Always
            : CookieSecurePolicy.SameAsRequest;
});

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.HttpOnly = HttpOnlyPolicy.Always;

    options.MinimumSameSitePolicy = SameSiteMode.Lax;

    options.Secure =
        forceHttps
            ? CookieSecurePolicy.Always
            : CookieSecurePolicy.SameAsRequest;
});

var keysPath = Path.Combine(builder.Environment.ContentRootPath, "Keys");

Directory.CreateDirectory(keysPath);
Log.Information("DataProtection keys path: {Path}", keysPath);
builder.Services
    .AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysPath))
    .SetApplicationName("PharmaPlus.Admin")
    .SetDefaultKeyLifetime(TimeSpan.FromDays(90));

// Authorization policies
builder.Services.AddAuthorizationBuilder()
    // Authorization policies
    .AddPolicy("AdminOnly", policy => policy.RequireRole(Roles.GetRoleBy(1), Roles.GetRoleBy(2)))
    // Authorization policies
    .AddPolicy("Support", policy => policy.RequireRole(Roles.GetRoleBy(1), Roles.GetRoleBy(2), Roles.GetRoleBy(3)))
    // Authorization policies
    .AddPolicy("ContentCreator", policy => policy.RequireRole(Roles.GetRoleBy(1), Roles.GetRoleBy(2), Roles.GetRoleBy(3), Roles.GetRoleBy(4)));

// --- 7. Razor Pages ---
builder.Services.AddRazorPages()
    .AddMvcOptions(options => options.Filters.Add<SecurityPageFilter>())
    .AddRazorPagesOptions(options =>
    {
        options.Conventions.AuthorizeAreaFolder("Admin", "/Accounts", "AdminOnly");
        options.Conventions.AuthorizeAreaFolder("Admin", "/Site", "AdminOnly");
        options.Conventions.AuthorizeAreaFolder("Admin", "/Shop", "Support");
        options.Conventions.AuthorizeAreaFolder("Admin", "/Inventory", "Support");
        options.Conventions.AuthorizeAreaFolder("Admin", "/Discounts", "Support");
        options.Conventions.AuthorizeAreaFolder("Admin", "/Comments", "ContentCreator");
        options.Conventions.AuthorizeAreaFolder("Admin", "/Blog", "ContentCreator");
    });

// --- 7. Controllers ---
builder.Services.AddControllers();

if (origins.Length == 0)
{
    throw new InvalidOperationException(
        "Cors:Origins is empty.");
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(origins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.Configure<SetupOptions>(
    builder.Configuration.GetSection("Setup"));

var app = builder.Build();

// --- Pipeline ---
await DatabaseSeeder.SeedAsync(app.Services);

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseRouting();

app.UseMiddleware<SetupBlockerMiddleware>();

var dashboardOptions = new DashboardOptions
{
    Authorization =
        app.Environment.IsDevelopment()
            ? []
            : new[] { new HangfireAuthorizationFilter() }
};

app.UseHangfireDashboard("/hangfire", dashboardOptions);

app.UseCors("Frontend");

if (forceHttps)
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseCookiePolicy();
app.UseMiddleware<OrderImagesAuthorizationMiddleware>();

app.UseStaticFiles();
app.UseIpRateLimiting();

RecurringJob.AddOrUpdate<AutoCancelPendingOrdersJob>(
    "Auto-Cancel-Pending-Orders",
    job => job.ProcessAsync(),
    "*/30 * * * *"
);

app.MapRazorPages();
app.MapControllers();
app.MapDefaultControllerRoute();

app.Run();
