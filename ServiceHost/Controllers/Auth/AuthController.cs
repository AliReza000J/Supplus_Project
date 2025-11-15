using AccountManagement.Application.Contracts.Account;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ServiceHost.Controllers.Auth
{
    [Route("auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAccountApplication _accountApp;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAccountApplication accountApp, ILogger<AuthController> logger)
        {
            _accountApp = accountApp;
            _logger = logger;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterAccount command)
        {
            try
            {
                var res = _accountApp.Register(command);
                return res.IsSucceeded ? Ok(res) : BadRequest(res);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا هنگام ثبت‌نام.");
                return Problem("خطای غیرمنتظره‌ای در هنگام ثبت‌نام رخ داد.");
            }
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] Login command)
        {
            try
            {
                var (res, data) = _accountApp.LoginWithJwt(command);
                return res.IsSucceeded ? Ok(data) : BadRequest(res);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا هنگام ورود.");
                return Problem("خطای غیرمنتظره‌ای در هنگام ورود رخ داد.");
            }
        }

        [HttpPost("refresh")]
        public IActionResult Refresh([FromBody] RefreshRequestDto request)
        {
            try
            {
                var (res, data) = _accountApp.RefreshToken(request.RefreshToken);
                return res.IsSucceeded ? Ok(data) : BadRequest(res);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا هنگام به‌روزرسانی توکن.");
                return Problem("خطای غیرمنتظره‌ای هنگام به‌روزرسانی توکن رخ داد.");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            try
            {
                var accessToken = Request.Headers["Authorization"]
                    .FirstOrDefault()?
                    .Split(" ")
                    .Last();

                if (string.IsNullOrWhiteSpace(accessToken))
                    return Unauthorized("توکن ارسال نشده است.");

                var res = _accountApp.LogoutWithJwt(accessToken);
                return res.IsSucceeded ? Ok(res) : BadRequest(res);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا هنگام خروج از حساب.");
                return Problem("خطای غیرمنتظره‌ای در هنگام خروج از حساب رخ داد.");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("me")]
        public IActionResult Me()
        {
            try
            {
                var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrWhiteSpace(idClaim))
                    return Unauthorized("شناسه کاربر یافت نشد.");

                if (!long.TryParse(idClaim, out var id))
                    return Unauthorized("شناسه کاربر نامعتبر است.");

                var model = _accountApp.GetAccountBy(id);
                return Ok(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا هنگام دریافت اطلاعات کاربر.");
                return Problem("خطای غیرمنتظره‌ای در هنگام دریافت اطلاعات کاربر رخ داد.");
            }
        }
    }
}
