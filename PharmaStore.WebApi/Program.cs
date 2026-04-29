using _0_Framework.Application;
using _0_Framework.Infrastructure;
using AccountManagement.Configuration;
using BlogManagement.Configuration;
using CommentManagement.Infrastructure.Configuration;
using DiscountManagement.Configuration;
using InventoryManagement.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using ShopManagement.Configuration;
using SiteManagement.Configuration;
using PharmaStore.WebApi.Filters;
using System.Text;
using PharmaStore.WebApi.Middlewares;
using PharmaShared.Web.Services;
using PharmaShared.Web.Middlewares;
using PharmaShared.Web;

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

// --- 4. Caching ---
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ICacheService, MemoryCacheService>();

// --- 5. Application Services ---
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<CustomJwtEvents>();

builder.Services.AddTransient<IFileUploader, FileUploader>();
builder.Services.AddTransient<IAuthHelper, AuthHelper>();

builder.Services.AddSingleton<IPasswordResetRateLimiter, PasswordResetRateLimiter>();
builder.Services.AddSingleton<IOtpRateLimiter, OtpRateLimiter>();

builder.Services.AddTransient<IEmailService, EmailService>();

builder.Services.AddScoped<IResetTokenGenerator, ResetTokenGenerator>();
builder.Services.AddSingleton<IResetTokenProtector>(sp => new HmacTokenProtector(sp.GetRequiredService<IConfiguration>().GetValue<string>("PasswordReset:HmacKey")));

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
    options.AddPolicy("AllowAll", policy => policy.WithOrigins("https://localhost:5173").AllowAnyMethod().AllowAnyHeader());
    options.AddPolicy("ApiCors", policy => policy.WithOrigins("https://pharmaplus.ir").AllowAnyMethod().AllowAnyHeader());
});

// --- 7. Auth (JWT Only) ---
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
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

if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowAll");
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseCors("ApiCors");
}

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();

app.MapControllers();

app.Run();
