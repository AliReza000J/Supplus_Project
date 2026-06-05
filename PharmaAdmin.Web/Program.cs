using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Features;
using Serilog;
using Serilog.Events;
using Hangfire;
using QuestPDF.Infrastructure;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using ShopManagement.Configuration;
using SiteManagement.Configuration;
using DiscountManagement.Configuration;
using InventoryManagement.Configuration;
using BlogManagement.Configuration;
using CommentManagement.Infrastructure.Configuration;
using AccountManagement.Configuration;
using _0_Framework.Application;
using _01_PharmaPlusQuery.Contracts.Dashbord;
using _0_Framework.Infrastructure;
using _01_PharmaPlusQuery.Query;
using Polly;
using AccountManagement.Domain.RoleAgg;
using PharmaAdmin.Web.Services;
using PharmaAdmin.Web.Filters;
using PharmaAdmin.Web.Middleware;
using PharmaShared.Web.Services;
using PharmaShared.Web.Middlewares;
using AspNetCoreRateLimit;
using PharmaShared.Web;
using _01_PharmaPlusQuery.Contracts.AdminNotification;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

var connectionString = config.GetConnectionString("PharmaPlusDB") ?? "";

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

builder.Services.AddHttpClient("SmsIr", client => { client.Timeout = TimeSpan.FromSeconds(10); });
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

builder.Services.AddHangfire(c => c.UseSqlServerStorage(config.GetConnectionString("Hangfire")));
builder.Services.AddHangfireServer();

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

builder.Services.AddScoped<IResetTokenGenerator, ResetTokenGenerator>();
builder.Services.AddSingleton<IResetTokenProtector>(sp => new HmacTokenProtector(sp.GetRequiredService<IConfiguration>().GetValue<string>("PasswordReset:HmacKey")));

builder.Services.AddScoped<ISmsService, SmsService>();
builder.Services.AddScoped<PostTrackingSmsJob>();

// Background tasks
builder.Services.AddScoped<ITokenCleanupService, TokenCleanupService>();
builder.Services.AddHostedService<TokenCleanupBackgroundService>();

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
    options.ExpireTimeSpan = TimeSpan.FromDays(3);
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole(Roles.GetRoleBy(1), Roles.GetRoleBy(2)));
    options.AddPolicy("Support", policy => policy.RequireRole(Roles.GetRoleBy(1), Roles.GetRoleBy(2), Roles.GetRoleBy(3)));
    options.AddPolicy("ContentCreator", policy => policy.RequireRole(Roles.GetRoleBy(1), Roles.GetRoleBy(2), Roles.GetRoleBy(3), Roles.GetRoleBy(4)));
});

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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy => policy.WithOrigins("http://localhost:5173").AllowAnyMethod().AllowAnyHeader());
    options.AddPolicy("ApiCors", policy => policy.WithOrigins("https://pharmaplus.ir").AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// --- Pipeline ---
await DatabaseSeeder.SeedAsync(app.Services);

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseRouting();
app.UseMiddleware<SetupBlockerMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseCookiePolicy();
app.UseMiddleware<OrderImagesAuthorizationMiddleware>();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseHangfireDashboard("/hangfire");
    app.UseCors("AllowAll");
}
else
{
    app.UseHangfireDashboard("/admin/hangfire", new DashboardOptions
    {
        Authorization = new[] { new HangfireAuthorizationFilter() }
    });
    app.UseCors("ApiCors");
}

app.UseIpRateLimiting();
app.MapRazorPages();
app.MapDefaultControllerRoute();

app.Run();
