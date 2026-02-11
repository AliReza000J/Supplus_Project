using _0_Framework.Application;
using AccountManagement.Application.Contracts.Account;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using ServiceHost.Filters;

namespace ServiceHost.Controllers.Auth
{
    /// <summary>
    /// Handles user authentication operations such as registration, login,
    /// token refresh, logout, and retrieving authenticated user information.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public partial class AuthController : ControllerBase
    {
        private readonly IOtpRateLimiter _otpRateLimiter;
        private readonly ISmsService _smsService;
        private readonly IMemoryCache _cache;
        private readonly IAccountApplication _accountApp;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/>.
        /// </summary>
        public AuthController(IAccountApplication accountApp, IMemoryCache cache, ISmsService smsService, IOtpRateLimiter otpRateLimiter)
        {
            _accountApp = accountApp;
            _cache = cache;
            _smsService = smsService;
            _otpRateLimiter = otpRateLimiter;
        }

        /// <summary>
        /// Sends a one-time password (OTP) to the specified mobile number
        /// for authentication purposes.
        /// </summary>
        /// <param name="command"></param>
        /// <returns>
        /// An HTTP response indicating whether the OTP message was sent successfully
        /// or if an error occurred during SMS delivery.
        /// </returns>
        /// <response code="200">
        /// OTP code sent successfully to the provided mobile number.
        /// </response>
        /// <response code="400">
        /// Failed to send OTP due to SMS service error or invalid mobile number.
        /// </response>
        [HttpPost("send-otp")]
        [RateLimit(Limit = 3, Seconds = 60)]
        public async Task<IActionResult> SendOtp([FromBody] SendOtpDto command)
        {
            var otpCode = new Random().Next(10000, 99999).ToString();

            _cache.Set(command.Mobile, otpCode, TimeSpan.FromMinutes(2));

            if (!_otpRateLimiter.CanSend(command.Mobile))
            {
                return BadRequest("تعداد درخواست‌های کد ورود بیش از حد مجاز است");
            }

            var sent = await _smsService.SendLoginOtpAsync(command.Mobile, otpCode);

            if (sent)
                return Ok(new { message = "کد تایید برای شما ارسال شد." });
            else
                return BadRequest("خطا در ارسال پیامک. لطفا بعدا تلاش کنید.");
        }

        /// <summary>
        /// Verifies the one-time password (OTP) sent to the user's mobile number.
        /// If the account does not exist, a new account will be created automatically,
        /// and the user will be logged in by issuing JWT access and refresh tokens.
        /// </summary>
        /// <param name="model">
        /// OTP verification details including the mobile number and the verification code.
        /// </param>
        /// <param name="cancellation"></param>
        /// <returns>
        /// A JWT authentication response containing access and refresh tokens
        /// if verification and login succeed, or an error message otherwise.
        /// </returns>
        /// <response code="200">
        /// OTP verified successfully and JWT tokens issued.
        /// </response>
        /// <response code="400">
        /// OTP is invalid or expired, or registration failed.
        /// </response>
        /// <response code="401">
        /// Authentication failed during login.
        /// </response>
        [HttpPost("verify-otp")]
        public async Task<ActionResult<ApiResponse<JwtResponseDto>>> VerifyOtp([FromBody] VerifyOtpDto model, CancellationToken cancellation)
        {
            if (!_cache.TryGetValue(model.Mobile, out string cachedCode))
            {
                return BadRequest(ApiResponse<JwtResponseDto>
                    .Fail("کد منقضی شده یا اشتباه است."));
            }

            if (cachedCode != model.Code)
            {
                return BadRequest(ApiResponse<JwtResponseDto>
                    .Fail("کد وارد شده صحیح نیست."));
            }

            _cache.Remove(model.Mobile);

            if (!await _accountApp.ExistsAccountByMobile(model.Mobile, cancellation))
            {
                var command = new RegisterByMobile { Mobile = model.Mobile };
                var registerOp = await _accountApp.RegisterByMobile(command, cancellation);

                if (!registerOp.IsSucceeded)
                    return BadRequest(ApiResponse<JwtResponseDto>
                        .Fail(registerOp.Message));
            }

            var (loginOp, data) = await _accountApp
                .LoginByMobileWithJwt(model.Mobile, cancellation);

            if (!loginOp.IsSucceeded || data == null)
                return Unauthorized(
                    ApiResponse<JwtResponseDto>
                    .Fail(loginOp.Message));

            return Ok(ApiResponse<JwtResponseDto>
                .Ok(data, loginOp.Message));
        }

        // ============================================================
        // Register
        // ============================================================

        /// <summary>
        /// Registers a new account and automatically logs in the user
        /// by issuing a JWT access token and refresh token.
        /// </summary>
        /// <param name="command">Registration details: username, password, mobile, etc.</param>
        /// <param name="cancellation"></param>
        /// <returns>
        /// A JWT authentication response containing access/refresh tokens, 
        /// or an error message if registration fails.
        /// </returns>
        /// <response code="200">Registration succeeded and token issued.</response>
        /// <response code="400">Validation error or registration failed.</response>
        [AllowAnonymous]
        [HttpPost("register")]
        [RateLimit(Limit = 3, Seconds = 60)]
        public async Task<ActionResult<ApiResponse<JwtResponseDto>>> Register(
            [FromForm] RegisterAccountApiDto command, CancellationToken cancellation)
        {
            var regCommand = command as RegisterAccount;
            var op = await _accountApp.Register(regCommand, cancellation);

            var res = new ApiResponse<JwtResponseDto>
            {
                Success = op.IsSucceeded,
                Message = op.Message,
            };

            if (!op.IsSucceeded)
                return BadRequest(res);

            var loginCommand = new Login
            {
                Username = command.Username,
                Password = command.Password
            };

            var (loginOp, data) = await _accountApp.LoginWithJwt(loginCommand, cancellation);

            if (!loginOp.IsSucceeded)
                return Unauthorized(res);

            res.Data = data;

            return Ok(res);
        }

        // ============================================================
        // Login
        // ============================================================

        /// <summary>
        /// Authenticates the user and returns a signed JWT access token
        /// along with a refresh token.
        /// </summary>
        /// <param name="command">Login credentials.</param>
        /// <param name="cancellation"></param>
        /// <returns>JWT tokens if authentication succeeds.</returns>
        /// <response code="200">Login successful.</response>
        /// <response code="401">Invalid credentials.</response>
        [AllowAnonymous]
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<JwtResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<JwtResponseDto>), StatusCodes.Status401Unauthorized)]
        [RateLimit(Limit = 5, Seconds = 60)]
        public async Task<ActionResult<ApiResponse<JwtResponseDto>>> Login([FromBody] Login command, CancellationToken cancellation)
        {
            var (op, data) = await _accountApp.LoginWithJwt(command, cancellation);

            if (!op.IsSucceeded)
                return Unauthorized(ApiResponse<JwtResponseDto>.Fail(message: op.Message));

            return Ok(ApiResponse<JwtResponseDto>.Ok(data: data, message: op.Message));
        }

        // ============================================================
        // Refresh Token
        // ============================================================

        /// <summary>
        /// Refreshes the JWT access token using a valid refresh token.
        /// </summary>
        /// <param name="request">Refresh token request data.</param>
        /// <param name="cancellation"></param>
        /// <returns>New access and refresh tokens.</returns>
        /// <response code="200">Token successfully refreshed.</response>
        /// <response code="401">Invalid or expired refresh token.</response>
        [RateLimit(Limit = 5, Seconds = 60)]
        [AllowAnonymous]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(ApiResponse<JwtResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<JwtResponseDto>), StatusCodes.Status401Unauthorized)]
        [RateLimit(Limit = 10, Seconds = 60)]
        public async Task<ActionResult<ApiResponse<JwtResponseDto>>> Refresh([FromBody] RefreshRequestDto request, CancellationToken cancellation)
        {
            var (op, data) = await _accountApp.RefreshToken(request.RefreshToken, cancellation);

            if (!op.IsSucceeded)
                return Unauthorized(ApiResponse<JwtResponseDto>.Fail(message: op.Message));

            return Ok(ApiResponse<JwtResponseDto>.Ok(data: data, message: op.Message));
        }

        // ============================================================
        // Logout
        // ============================================================

        /// <summary>
        /// Logs out the user by invalidating the provided JWT access token.
        /// </summary>
        /// <remarks>
        /// This action requires a valid <c>Bearer</c> access token.
        /// </remarks>
        /// <returns>Success or failure response.</returns>
        /// <response code="200">Logout successful.</response>
        /// <response code="400">Logout failed.</response>
        /// <response code="401">Token missing or invalid.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("logout")]
        public async Task<ActionResult<ApiResponse<object>>> Logout(CancellationToken cancellation)
        {
            var accessToken = Request.Headers["Authorization"]
                    .FirstOrDefault()?
                    .Split(" ")
                    .Last();

            if (string.IsNullOrWhiteSpace(accessToken))
                return Unauthorized(ApiResponse<JwtResponseDto>.Fail(message: "توکن ارسال نشده است."));

            var op = await _accountApp.LogoutWithJwt(accessToken, cancellation);

            var res = new ApiResponse<object>
            {
                Success = op.IsSucceeded,
                Message = op.Message
            };

            return res.Success ? Ok(res) : BadRequest(res);
        }
    }
}
