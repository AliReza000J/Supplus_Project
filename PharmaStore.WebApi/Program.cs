using _0_Framework.Application;
using _0_Framework.Infrastructure;
using AccountManagement.Configuration;
using AspNetCoreRateLimit;
using BlogManagement.Configuration;
using CommentManagement.Infrastructure.Configuration;
using DiscountManagement.Configuration;
using InventoryManagement.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PharmaShared.Web;
using PharmaShared.Web.Middlewares;
using PharmaShared.Web.Services;
using PharmaStore.WebApi.Filters;
using PharmaStore.WebApi.Middlewares;
using Polly;
using QuestPDF.Infrastructure;
using Serilog;
using Serilog.Events;
using ShopManagement.Configuration;
using SiteManagement.Configuration;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration;

var forceHttps =
    config.GetValue<bool>("Security:ForceHttps");

var origins =
    config.GetSection("Cors:Origins")
          .Get<string[]>() ?? [];

if (origins.Length == 0)
{
    throw new InvalidOperationException(
        "Cors:Origins is empty.");
}

var connectionString =
    config.GetConnectionString("PharmaPlusDB")
    ?? throw new InvalidOperationException(
        "Connection string 'PharmaPlusDB' was not found.");

var jwtKey =
    config["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "Jwt:Key is missing.");

var jwtIssuer =
    config["Jwt:Issuer"]
    ?? throw new InvalidOperationException(
        "Jwt:Issuer is missing.");

var jwtAudience =
    config["Jwt:Audience"]
    ?? throw new InvalidOperationException(
        "Jwt:Audience is missing.");

// --- 1. Logging (Serilog) ---
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Filter.ByExcluding(le => le.MessageTemplate.Text.Contains("Authorization"))
    .WriteTo.Console()
    .WriteTo.File("Logs/api/api-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 90, restrictedToMinimumLevel: LogEventLevel.Information, shared: true)
    .WriteTo.File("Logs/errors/error-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 90, restrictedToMinimumLevel: LogEventLevel.Error, shared: true)
    .CreateLogger();

builder.Host.UseSerilog();

// --- 2. HTTP Clients & Base Services ---
builder.Services.AddHttpClient("SmsIr", client => { client.Timeout = TimeSpan.FromSeconds(10); });
builder.Services.AddHttpContextAccessor();

// --- 3. Bootstrappers (IoC) ---
ShopManagementBootstrapper.Configure(builder.Services, connectionString);
SiteManagementBootstrapper.Configure(builder.Services, connectionString);
DiscountManagementBootstrapper.Configure(builder.Services, connectionString);
InventoryManagementBootstrapper.Configure(builder.Services, connectionString);
BlogManagementBootstrapper.Configure(builder.Services, connectionString);
CommentManagementBootstrapper.Configure(builder.Services, connectionString);
AccountManagementBootstrapper.Configure(builder.Services, connectionString);

// --- 4. Caching & IPRateLimiter ---
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ICacheService, MemoryCacheService>();

QuestPDF.Settings.License = LicenseType.Community;

builder.Services.Configure<IpRateLimitOptions>(config.GetSection("IpRateLimiting"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// --- 5. Application Services ---
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<CustomJwtEvents>();

builder.Services.AddTransient<IFileUploader, FileUploader>();
builder.Services.AddTransient<IAuthHelper, AuthHelper>();

builder.Services.AddSingleton<IOtpRateLimiter, OtpRateLimiter>();
builder.Services.AddSingleton<IActionRateLimiter, InMemoryRateLimiter>();

builder.Services.AddTransient<IEmailService, EmailService>();

builder.Services.AddHttpClient<IZibalService, ZibalService>();
builder.Services.AddScoped<ISmsService, SmsService>();

// --- 6. API Controllers & CORS ---
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ApiValidationFilter>();
})
.ConfigureApiBehaviorOptions(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// --- 7. Auth (JWT Only) ---
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.RequireHttpsMetadata = forceHttps;
    
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = jwtIssuer,

        ValidAudience = jwtAudience,

        IssuerSigningKey =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)),

        ClockSkew = TimeSpan.Zero
    };
    options.EventsType = typeof(CustomJwtEvents);
});

// --- 8. Swagger ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "PharmaPlus Shop API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Bearer token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    var xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml");
    foreach (var filePath in xmlFiles)
    {
        c.IncludeXmlComments(filePath, includeControllerXmlComments: true);
    }

    c.OperationFilter<SwaggerDefaultResponsesOperationFilter>();
    c.OperationFilter<AddAuthRequirementOperationFilter>();
});

var app = builder.Build();

// --- Pipeline ---
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseRouting();

app.UseMiddleware<SetupBlockerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();
app.UseIpRateLimiting();
app.MapControllers();

app.Run();
