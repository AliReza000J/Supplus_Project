using _0_Framework.Application;
using AccountManagement.Application.Contracts.Account;
using AccountManagement.Application.Contracts.Account.ApiDTOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PharmaStore.WebApi.Controllers.Auth
{
    /// <summary>
    /// Manages user authentication, registration, token handling, and password recovery processes.
    /// </summary>
    /// <remarks>
    /// This controller provides endpoints for user login (with password or OTP), registration,
    /// OTP verification, password reset, token refreshing, and user logout.
    /// It includes rate limiting on sensitive endpoints to enhance security.
    /// </remarks>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAccountApplication _accountApp;
        private readonly IOtpRateLimiter _otpRateLimiter;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/>.
        /// </summary>
        public AuthController(IAccountApplication accountApp, IOtpRateLimiter otpRateLimiter)
        {
            _accountApp = accountApp;
            _otpRateLimiter = otpRateLimiter;
        }

        /// <summary>
        /// Sends a one-time password (OTP) to the user's mobile for login or registration.
        /// </summary>
        /// <remarks>
        /// This endpoint initiates the login/registration process. If the mobile number is not registered,
        /// a new user account will be prepared. A text message containing the verification code is sent to the provided mobile number.
        /// <br/>
        /// </remarks>
        /// <param name="command">The DTO containing the user's mobile number.</param>
        /// <param name="cancellation">A token to cancel the operation.</param>
        /// <response code="200">OTP sent successfully.</response>
        /// <response code="400">Invalid mobile number format or another error occurred.</response>
        [AllowAnonymous]
        [HttpPost("login/send-otp")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult<ApiResponse<object>>> SendLoginOtp([FromBody] SendOtpDto command, CancellationToken cancellation)
        {
            if (!_otpRateLimiter.CanSend(command.Mobile))
                return StatusCode(StatusCodes.Status429TooManyRequests,
                ApiResponse<object>.Fail("تعداد درخواست‌ها بیش از حد مجاز است. لطفا دقایقی دیگر تلاش کنید."));

            var result = await _accountApp.SendLoginOtp(command.Mobile, cancellation);
            return result.IsSucceeded
                ? Ok(ApiResponse<object>.Ok(new(), message: result.Message))
                : BadRequest(ApiResponse<object>.Fail(result.Message));
        }

        /// <summary>
        /// Verifies the OTP to complete the login or registration process.
        /// </summary>
        /// <remarks>
        /// After receiving an OTP via the 'send-otp' endpoint, the user submits their mobile number and the received code to this endpoint.
        /// If the code is valid, the system either logs the existing user in or finalizes the registration for a new user,
        /// returning JWT (Access and Refresh) tokens in both cases.
        /// </remarks>
        /// <param name="model">The DTO containing the user's mobile number and the OTP code.</param>
        /// <param name="cancellation">A token to cancel the operation.</param>
        /// <response code="200">Login/Registration successful. Returns JWT tokens.</response>
        /// <response code="401">Unauthorized. The provided OTP is incorrect, expired, or the mobile number is invalid.</response>
        [AllowAnonymous]
        [HttpPost("login/verify-otp")]
        [ProducesResponseType(typeof(ApiResponse<JwtResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<JwtResponseDto>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<JwtResponseDto>>> VerifyLoginOtp([FromBody] VerifyOtpDto model, CancellationToken cancellation)
        {
            var (result, data) = await _accountApp.LoginOrRegisterWithOtp(model.Mobile, model.Code, cancellation);
            if (!result.IsSucceeded || data == null) return Unauthorized(ApiResponse<JwtResponseDto>.Fail(result.Message));
            return Ok(ApiResponse<JwtResponseDto>.Ok(data, result.Message));
        }

        /// <summary>
        /// Sends a one-time password (OTP) to the user's mobile for resetting their password.
        /// </summary>
        /// <remarks>
        /// This is the first step in the "forgot password" flow. The user provides their mobile number,
        /// and if an account exists, an OTP is sent to them to verify their identity.
        /// <br/>
        /// </remarks>
        /// <param name="command">The DTO containing the user's mobile number.</param>
        /// <param name="cancellation">A token to cancel the operation.</param>
        /// <response code="200">OTP for password reset sent successfully.</response>
        /// <response code="400">Mobile number is not registered, has an invalid format, or another error occurred.</response>
        [AllowAnonymous]
        [HttpPost("forgot-password/send-otp")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult<ApiResponse<object>>> SendResetPasswordOtp([FromBody] SendOtpDto command, CancellationToken cancellation)
        {
            if (!_otpRateLimiter.CanSend(command.Mobile))
                return StatusCode(StatusCodes.Status429TooManyRequests,
                ApiResponse<object>.Fail("تعداد درخواست‌ها بیش از حد مجاز است. لطفا دقایقی دیگر تلاش کنید."));

            var result = await _accountApp.SendResetPasswordOtp(command.Mobile, cancellation);
            return result.IsSucceeded
                ? Ok(ApiResponse<object>.Ok(new(), message: result.Message))
                : BadRequest(ApiResponse<object>.Fail(result.Message));
        }

        /// <summary>
        /// Resets the user's password after verifying an OTP.
        /// </summary>
        /// <remarks>
        /// This is the final step in the password reset process. The user submits their mobile number, the OTP they received, 
        /// and their new password. If the code is valid, the system updates the user's password.
        /// </remarks>
        /// <param name="command">The DTO containing the mobile number, OTP code, and the new password details.</param>
        /// <param name="cancellation">A token to cancel the operation.</param>
        /// <response code="200">Password successfully reset.</response>
        /// <response code="400">Bad request. The provided OTP is incorrect/expired, or the new password validation failed.</response>
        [AllowAnonymous]
        [HttpPost("forgot-password/verify-and-reset")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<object>>> ResetPasswordWithOtp([FromBody] ResetPasswordWithOtpApi command, CancellationToken cancellation)
        {
            var result = await _accountApp.ResetPasswordWithOtp(command, cancellation);
            return result.IsSucceeded
                ? Ok(ApiResponse<object>.Ok(new(), message: result.Message))
                : BadRequest(ApiResponse<object>.Fail(result.Message));
        }

        /// <summary>
        /// Authenticates a user with a username/mobile and password.
        /// </summary>
        /// <remarks>
        /// This endpoint allows users to log in by providing their credentials. 
        /// Upon successful authentication, it returns a JWT (JSON Web Token) and a refresh token.
        /// <br/>
        /// </remarks>
        /// <param name="command">The login credentials DTO, containing the username/mobile and password.</param>
        /// <param name="rateLimiter"></param>
        /// <param name="cancellation">A token to cancel the operation.</param>
        /// <response code="200">Login successful. Returns JWT and refresh tokens.</response>
        /// <response code="401">Unauthorized. Invalid username/mobile or password.</response>
        [AllowAnonymous]
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<JwtResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<JwtResponseDto>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<JwtResponseDto>>> Login([FromBody] LoginToApiViaUserMobilePass command, [FromServices] IActionRateLimiter rateLimiter, CancellationToken cancellation)
        {
            if (!rateLimiter.CheckLimit("Login", command.UsernameOrMobile, maxRequests: 5, timeWindow: TimeSpan.FromMinutes(15)))
            {
                return StatusCode(StatusCodes.Status429TooManyRequests,
                    ApiResponse<JwtResponseDto>.Fail("تعداد درخواست‌های لاگین بیش از حد مجاز است. لطفا 15 دقیقه دیگر تلاش کنید."));
            }

            var (op, data) = await _accountApp.LoginToStoreViaUserPass(command, cancellation);

            if (!op.IsSucceeded || data == null)
                return Unauthorized(ApiResponse<JwtResponseDto>.Fail(message: op.Message));

            return Ok(ApiResponse<JwtResponseDto>.Ok(data: data, message: op.Message));
        }

        /// <summary>
        /// Refreshes an expired JWT using a valid refresh token.
        /// </summary>
        /// <remarks>
        /// This endpoint takes a refresh token and, if it's valid and not expired, returns a new JWT and a new refresh token.
        /// This allows the user to maintain their session without needing to log in again.
        /// <br/>
        /// </remarks>
        /// <param name="request">The request body containing the refresh token.</param>
        /// <param name="cancellation">A token to cancel the operation.</param>
        /// <response code="200">Token refresh successful. Returns a new JWT and refresh token.</response>
        /// <response code="401">Unauthorized. The refresh token is invalid, expired, or has been revoked.</response>
        [AllowAnonymous]
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(ApiResponse<JwtResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<JwtResponseDto>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<JwtResponseDto>>> Refresh([FromBody] RefreshRequestDto request, CancellationToken cancellation)
        {
            var (op, data) = await _accountApp.RefreshToken(request.RefreshToken, cancellation);

            if (!op.IsSucceeded || data == null)
                return Unauthorized(ApiResponse<JwtResponseDto>.Fail(message: op.Message));

            return Ok(ApiResponse<JwtResponseDto>.Ok(data: data, message: op.Message));
        }

        /// <summary>
        /// Logs out the currently authenticated user.
        /// </summary>
        /// <remarks>
        /// This endpoint invalidates the user's current access token, effectively logging them out from the application.
        /// The access token must be provided in the 'Authorization' header as a Bearer token.
        /// </remarks>
        /// <param name="cancellation">A token to cancel the operation.</param>
        /// <response code="200">User logged out successfully.</response>
        /// <response code="400">Logout failed due to a server-side issue.</response>
        /// <response code="401">User is not authenticated or the token is missing/invalid.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("logout")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<object>>> Logout(CancellationToken cancellation)
        {
            var accessToken = Request.Headers["Authorization"]
                    .FirstOrDefault()?
                    .Split(" ")
                    .Last();

            if (string.IsNullOrWhiteSpace(accessToken))
                return Unauthorized(ApiResponse<object>.Fail(message: "توکن ارسال نشده است."));

            var op = await _accountApp.LogoutFromStore(accessToken, cancellation);

            var res = new ApiResponse<object>
            {
                Success = op.IsSucceeded,
                Message = op.Message
            };

            return res.Success ? Ok(res) : BadRequest(res);
        }
    }

}
