using _0_Framework.Application;
using AccountManagement.Configuration;
using AccountManagement.Domain.RoleAgg;
using AccountManagement.Domain.TokenAgg;
using BlogManagement.Configuration;
using CommentManagement.Infrastructure.Configuration;
using DiscountManagement.Configuration;
using InventoryManagement.Configuration;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ServiceHost;
using ShopManagement.Configuration;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Unicode;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

var connectionString = builder.Configuration.GetConnectionString("PharmaPlusDB");

builder.Services.AddHttpContextAccessor();

ShopManagementBootstrapper.Configure(builder.Services, connectionString);
DiscountManagementBootstrapper.Configure(builder.Services, connectionString);
InventoryManagementBootstrapper.Configure(builder.Services, connectionString);
BlogManagementBootstrapper.Configure(builder.Services, connectionString);
CommentManagementBootstrapper.Configure(builder.Services, connectionString);
AccountManagementBootstrapper.Configure(builder.Services, connectionString);

// Add services to the container.
builder.Services.AddTransient<IFileUploader, FileUploader>();
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IAuthHelper, AuthHelper>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Arabic));

builder.Services.AddRazorPages();
builder.Services.AddControllers();

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
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (!string.IsNullOrWhiteSpace(token))
            {
                var repo = context.HttpContext.RequestServices.GetService<ITokenRepository>();
                if (repo != null && repo.IsRevoked(token))
                {
                    context.Fail("این توکن باطل شده است.");
                }
            }
            return Task.CompletedTask;
        }
    };
});

// Authorization policies (example using permission claim or role)
builder.Services.AddAuthorization(options =>
{
    // example: require roleId=1 (Admin)
    options.AddPolicy("AdminOnly", policy => policy.RequireClaim(ClaimTypes.Role, Roles.GetRoleBy(1)));
    options.AddPolicy("Support", policy => policy.RequireClaim(ClaimTypes.Role, new List<string> { Roles.GetRoleBy(1), Roles.GetRoleBy(3) }));
    options.AddPolicy("ContentUploader", policy => policy.RequireClaim(ClaimTypes.Role, new List<string> { Roles.GetRoleBy(1), Roles.GetRoleBy(3), Roles.GetRoleBy(4) }));

    //options.AddPolicy("AdminOnly", policy => policy.RequireRole(new List<string> { "1"}));
});

builder.Services.AddRazorPages()
    .AddMvcOptions(options => options.Filters.Add<SecurityPageFilter>())
    .AddRazorPagesOptions(options =>
    {
        options.Conventions.AuthorizeAreaFolder("Admin", "/Accounts", "AdminOnly");
        ///
        options.Conventions.AuthorizeAreaFolder("Admin", "/Shop", "Support");
        options.Conventions.AuthorizeAreaFolder("Admin", "/Blog", "Support");
        options.Conventions.AuthorizeAreaFolder("Admin", "/Comments", "Support");
        options.Conventions.AuthorizeAreaFolder("Admin", "/Inventory", "Support");
        options.Conventions.AuthorizeAreaFolder("Admin", "/Discounts", "Support");
        ///
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

var app = builder.Build();

app.UseCors("AllowAll");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PharmaPlus Shop API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseAuthentication();

app.UseStaticFiles();

app.UseCookiePolicy();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();   // Admin
app.MapDefaultControllerRoute();
app.MapControllers();  // API

app.Run();
