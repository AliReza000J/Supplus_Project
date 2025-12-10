using _0_Framework.Application;
using AccountManagement.Application.Contracts.Account;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopManagement.Application.Contracts.Cart;
using System.Security.Claims;

namespace ServiceHost.Controllers.Auth
{
    /// <summary>
    /// Handles user authentication operations such as registration, login,
    /// token refresh, logout, and retrieving authenticated user information.
    /// </summary>
    [Route("auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAccountApplication _accountApp;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/>.
        /// </summary>
        public AuthController(IAccountApplication accountApp)
        {
            _accountApp = accountApp;
        }

        // ============================================================
        // Register
        // ============================================================

        /// <summary>
        /// Registers a new account and automatically logs in the user
        /// by issuing a JWT access token and refresh token.
        /// </summary>
        /// <param name="command">Registration details: username, password, mobile, etc.</param>
        /// <returns>
        /// A JWT authentication response containing access/refresh tokens, 
        /// or an error message if registration fails.
        /// </returns>
        /// <response code="200">Registration succeeded and token issued.</response>
        /// <response code="400">Validation error or registration failed.</response>
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<JwtResponseDto>>> Register([FromBody] RegisterAccount command)
        {
            var op = await _accountApp.Register(command);

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

            var (loginOp, data) = await _accountApp.LoginWithJwt(loginCommand);

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
        /// <returns>JWT tokens if authentication succeeds.</returns>
        /// <response code="200">Login successful.</response>
        /// <response code="401">Invalid credentials.</response>
        [AllowAnonymous]
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<JwtResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<JwtResponseDto>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<JwtResponseDto>>> Login([FromBody] Login command)
        {
            var (op, data) = await _accountApp.LoginWithJwt(command);

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
        /// <returns>New access and refresh tokens.</returns>
        /// <response code="200">Token successfully refreshed.</response>
        /// <response code="401">Invalid or expired refresh token.</response>
        [AllowAnonymous]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(ApiResponse<JwtResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<JwtResponseDto>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<JwtResponseDto>>> Refresh([FromBody] RefreshRequestDto request)
        {
            var (op, data) = await _accountApp.RefreshToken(request.RefreshToken);

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
        public async Task<ActionResult<ApiResponse<object>>> Logout()
        {
            var accessToken = Request.Headers["Authorization"]
                    .FirstOrDefault()?
                    .Split(" ")
                    .Last();

            if (string.IsNullOrWhiteSpace(accessToken))
                return Unauthorized(ApiResponse<JwtResponseDto>.Fail(message: "توکن ارسال نشده است."));

            var op = await _accountApp.LogoutWithJwt(accessToken);

            var res = new ApiResponse<object>
            {
                Success = op.IsSucceeded,
                Message = op.Message
            };

            return res.Success ? Ok(res) : BadRequest(res);
        }

    }
}
