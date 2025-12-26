using _0_Framework.Application;
using _0_Framework.Infrastructure;
using AccountManagement.Configuration;
using AccountManagement.Domain.RoleAgg;
using BlogManagement.Configuration;
using CommentManagement.Infrastructure.Configuration;
using DiscountManagement.Configuration;
using Hangfire;
using InventoryManagement.Configuration;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Polly;
using QuestPDF.Drawing;
using QuestPDF.Infrastructure;
using Serilog;
using Serilog.Events;
using ServiceHost;
using ServiceHost.Filters;
using ServiceHost.Middleware;
using ServiceHost.Services;
using ShopManagement.Configuration;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Unicode;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

var connectionString = config.GetConnectionString("PharmaPlusDB") ?? "";

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

Log.Logger = new LoggerConfiguration()

    .MinimumLevel.Information()

    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)

    .Enrich.FromLogContext()

    .Filter.ByExcluding(le =>
    le.MessageTemplate.Text.Contains("Authorization"))

    .WriteTo.Console()

    .WriteTo.File(
        path: "Logs/app/app-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 90,
        restrictedToMinimumLevel: LogEventLevel.Information,
        shared: true
    )

    .WriteTo.File(
        path: "Logs/errors/error-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 90,
        restrictedToMinimumLevel: LogEventLevel.Error,
        shared: true
    )

    .CreateLogger();

builder.Host.UseSerilog();


ShopManagementBootstrapper.Configure(builder.Services, connectionString);
DiscountManagementBootstrapper.Configure(builder.Services, connectionString);
InventoryManagementBootstrapper.Configure(builder.Services, connectionString);
BlogManagementBootstrapper.Configure(builder.Services, connectionString);
CommentManagementBootstrapper.Configure(builder.Services, connectionString);
AccountManagementBootstrapper.Configure(builder.Services, connectionString);

//builder.Services.AddMemoryCache(options =>
//{
//    //options.SizeLimit = 512;
//});
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ICacheService, MemoryCacheService>();

builder.Services.AddHangfire(config =>
{
    config.UseSqlServerStorage(
        builder.Configuration.GetConnectionString("Hangfire"));
});

builder.Services.AddHangfireServer();


QuestPDF.Settings.License = LicenseType.Community;

var fontPath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "AdminTheme", "fonts", "IranianSans.ttf"); if (File.Exists(fontPath))
    
    if (File.Exists(fontPath))
    {
        using var fontStream = File.OpenRead(fontPath);
        FontManager.RegisterFont(fontStream);
        // برای اطمینان در کنسول چاپ میکنیم
        Console.WriteLine("--- IranianSans Font Registered Successfully ---");
    }
    else
    {
        Console.WriteLine($"--- FONT NOT FOUND AT: {fontPath} ---");
    }

// Add services to the container.
builder.Services.AddTransient<IFileUploader, FileUploader>();
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IAuthHelper, AuthHelper>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Arabic));

builder.Services.AddSingleton<IOtpRateLimiter, OtpRateLimiter>();
builder.Services.AddScoped<IAdminSmsRateLimiter, AdminSmsRateLimiter>();

builder.Services.AddHttpClient<IZibalService, ZibalService>();

builder.Services.AddScoped<ISmsService, SmsService>();

builder.Services.AddScoped<ITokenCleanupService, TokenCleanupService>();
builder.Services.AddHostedService<TokenCleanupBackgroundService>();

builder.Services.AddScoped<PostTrackingSmsJob>();

builder.Services.AddRazorPages();
//builder.Services.AddControllers();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ApiValidationFilter>(); // your custom filter
})
.ConfigureApiBehaviorOptions(options =>
{
    options.SuppressModelStateInvalidFilter = true; // DISABLE default ProblemDetails
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()    // Allow requests from any domain
            .AllowAnyMethod()    // Allow all HTTP methods (GET, POST, etc.)
            .AllowAnyHeader();   // Allow all headers
    });
});

builder.Services.AddScoped<CustomJwtEvents>();

// Auth
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
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.RequireHttpsMetadata = true;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = config["Jwt:Issuer"],
        ValidAudience = config["Jwt:Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"])),
        ClockSkew = TimeSpan.Zero
    };
    options.EventsType = typeof(CustomJwtEvents);
});

// Authorization policies (example using permission claim or role)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole(
            Roles.GetRoleBy(1),
            Roles.GetRoleBy(2)
            ));

    options.AddPolicy("Seller", policy =>
        policy.RequireRole(
            Roles.GetRoleBy(1),
            Roles.GetRoleBy(2),
            Roles.GetRoleBy(3)
        ));

    options.AddPolicy("ContentUploader", policy =>
        policy.RequireRole(
            Roles.GetRoleBy(1),
            Roles.GetRoleBy(2),
            Roles.GetRoleBy(3),
            Roles.GetRoleBy(4)
        ));
});

builder.Services.AddRazorPages()
    .AddMvcOptions(options => options.Filters.Add<SecurityPageFilter>())
    .AddRazorPagesOptions(options =>
    {
        //
        options.Conventions.AuthorizeAreaFolder("Admin", "/Accounts", "AdminOnly");
        //
        options.Conventions.AuthorizeAreaFolder("Admin", "/Shop", "Seller");
        options.Conventions.AuthorizeAreaFolder("Admin", "/Inventory", "Seller");
        options.Conventions.AuthorizeAreaFolder("Admin", "/Discounts", "Seller");
        //
        options.Conventions.AuthorizeAreaFolder("Admin", "/Comments", "ContentUploader");
        options.Conventions.AuthorizeAreaFolder("Admin", "/Blog", "ContentUploader");
    });
//.AddRazorPagesOptions(options =>
//{
//    options.Conventions.AddPageRoute("/Admin/AccessDenied", "AccessDenied");
//});


// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PharmaPlus Shop API",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Bearer token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    // Include XML comments (for better docs)
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

    // OperationFilter for adding common responses (401/403) or examples (see below)
    c.OperationFilter<SwaggerDefaultResponsesOperationFilter>();
    c.OperationFilter<AddAuthRequirementOperationFilter>();

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseCookiePolicy();
app.UseCors("AllowAll");
app.UseHangfireDashboard("/hangfire");

app.MapControllers();
app.MapRazorPages();
app.MapDefaultControllerRoute();

app.Run();
