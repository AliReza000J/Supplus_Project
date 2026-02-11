using _0_Framework.Application;
using _01_PharmaPlusQuery.Contracts.Order;
using _01_PharmaPlusQuery.Contracts.Product;
using AccountManagement.Application.Contracts.Account;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceHost.Filters;
using System.Security.Claims;

namespace ServiceHost.Controllers.Account
{
    /// <summary>
    /// Handles user account operations such as edit or complete profile, change password,
    /// and retrieving authenticated user information, previous orders and favorited items.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountApplication _accountApp;
        private readonly IProductQuery _productQuery;
        private readonly IOrderQuery _orderQuery;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/>.
        /// </summary>
        public AccountController(IAccountApplication accountApp,
            IProductQuery productQuery,
            IOrderQuery orderQuery)
        {
            _accountApp = accountApp;
            _productQuery = productQuery;
            _orderQuery = orderQuery;
        }
        // ============================================================
        // ME
        // ============================================================

        /// <summary>
        /// Retrieves the authenticated user's profile information
        /// based on the <c>NameIdentifier</c> claim inside the JWT token.
        /// </summary>
        /// <returns>User profile information.</returns>
        /// <response code="200">User details retrieved successfully.</response>
        /// <response code="401">Token missing or invalid.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("me")]
        public async Task<ActionResult<ApiResponse<AccountViewModel>>> Me(CancellationToken cancellation)
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(idClaim))
                return Unauthorized(ApiResponse<AccountViewModel>.Fail("شناسه کاربر یافت نشد."));

            if (!long.TryParse(idClaim, out var userId))
                return Unauthorized(ApiResponse<AccountViewModel>.Fail($"شناسه کاربری {userId} نامعتبر است."));

            var model = await _accountApp.GetAccountBy(userId, cancellation);

            return Ok(ApiResponse<AccountViewModel>.Ok(model, "اطلاعات با موفقیت بازیابی شد."));
        }

        // ============================================================
        // FAVORITES
        // ============================================================

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("favorites")]
        public async Task<ActionResult<ApiResponse<List<ProductQueryModel>>>> UserFavoritedProdcts(CancellationToken cancellationToken)
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(idClaim))
                return Unauthorized(ApiResponse<ProductQueryModel>.Fail("شناسه کاربر یافت نشد."));

            if (!long.TryParse(idClaim, out var userId))
                return Unauthorized(ApiResponse<ProductQueryModel>.Fail($"شناسه کاربری {userId} نامعتبر است."));

            var favs = await _productQuery.GetUserFavoriteList(userId, cancellationToken);

            return Ok(ApiResponse<List<ProductQueryModel>>.Ok(
                favs,
                favs.Any()
                    ? "عملیات با موفقیت انجام شد."
                    : $"هیچ محصولی در لیست علاقه‌مندی شما یافت نشد!."
            ));
        }

        // ============================================================
        // ORDERS
        // ============================================================

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("orders")]
        public async Task<ActionResult<ApiResponse<List<OrderQueryModel>>>> GetUserOrders(CancellationToken cancellation)
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(idClaim))
                return Unauthorized(ApiResponse<OrderQueryModel>.Fail("شناسه کاربر یافت نشد."));

            if (!long.TryParse(idClaim, out var userId))
                return Unauthorized(ApiResponse<OrderQueryModel>.Fail($"شناسه کاربری {userId} نامعتبر است."));

            var orders = await _orderQuery.GetOrdersByUser(userId, cancellation);

            return Ok(ApiResponse<List<OrderQueryModel>>.Ok(
                orders,
                orders.Any()
                    ? "عملیات با موفقیت انجام شد."
                    : $"هیچ محصولی در لیست سفارشات پیشین شما یافت نشد!."
            ));
        }

        // ============================================================
        // COMPLETE PROFILE
        // ============================================================

        /// <summary>
        /// Completes the authenticated user's profile information.
        /// This endpoint requires a valid JWT access token.
        /// </summary>
        /// <param name="command">
        /// Profile completion details provided by the user.
        /// The user identifier is automatically extracted from the JWT token.
        /// </param>
        /// <param name="cancellation"></param>
        /// <returns>
        /// A standard API response indicating whether the profile completion
        /// operation was successful or not.
        /// </returns>
        /// <response code="200">Profile completed successfully.</response>
        /// <response code="400">Profile completion failed due to validation or business rules.</response>
        /// <response code="401">User is not authenticated or JWT token is invalid.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("complete-profile")]
        [RateLimit(Limit = 5, Seconds = 60)]
        public async Task<ActionResult<ApiResponse<object>>> CompleteProfile(
            [FromForm] CompleteProfileDto dto, CancellationToken cancellation)
        {
            var command = new CompleteProfile
            {
                Email = dto.Email,
                Fullname = dto.Fullname,
                Password = dto.Password,
                RePassword = dto.RePassword,
                ProfilePhoto = dto.ProfilePhoto,
                Username = dto.Username
            };

            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (long.TryParse(idClaim, out var parsedId))
                command.Id = parsedId;
            else
                command.Id = 0;

            var op = await _accountApp.CompleteProfile(command, cancellation);

            return op.IsSucceeded
                ? Ok(ApiResponse<object>.Ok(new(), message: op.Message))
                : BadRequest(ApiResponse<object>.Fail(op.Message));
        }

        // ============================================================
        // EDIT
        // ============================================================

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("edit-profile")]
        [RateLimit(Limit = 5, Seconds = 60)]
        public async Task<ActionResult<ApiResponse<object>>> EditProfile(
            [FromForm] EditAccountDto dto, CancellationToken cancellation)
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(idClaim))
                return Unauthorized(ApiResponse<OrderQueryModel>.Fail("شناسه کاربر یافت نشد."));

            if (!long.TryParse(idClaim, out var userId))
                return Unauthorized(ApiResponse<OrderQueryModel>.Fail($"شناسه کاربری {userId} نامعتبر است."));

            var command = new EditAccount
            {
                Email = dto.Email,
                Id = userId,
                Fullname = dto.Fullname,
                Username = dto.Username,
                Mobile = dto.Mobile,
                ProfilePhoto = dto.ProfilePhoto,
            };

            var op = await _accountApp.Edit(command, userId, cancellation);

            return op.IsSucceeded
                ? Ok(ApiResponse<object>.Ok(new(), message: op.Message))
                : BadRequest(ApiResponse<object>.Fail(op.Message));
        }

        // ============================================================
        // CHANGE PASSWORD
        // ============================================================

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("change-password")]
        [RateLimit(Limit = 3, Seconds = 60)]
        public async Task<ActionResult<ApiResponse<object>>> ChangePassword(
            [FromForm] ChangePasswordDto dto, CancellationToken cancellation)
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(idClaim))
                return Unauthorized(ApiResponse<OrderQueryModel>.Fail("شناسه کاربر یافت نشد."));

            if (!long.TryParse(idClaim, out var userId))
                return Unauthorized(ApiResponse<OrderQueryModel>.Fail($"شناسه کاربری {userId} نامعتبر است."));
            var command = new ChangePassword
            {
                Id = userId,
                Password = dto.Password,
                RePassword = dto.RePassword
            };
            var op = await _accountApp.ChangePassword(command, userId, cancellation);

            return op.IsSucceeded
                ? Ok(ApiResponse<object>.Ok(new(), message: op.Message))
                : BadRequest(ApiResponse<object>.Fail(op.Message));
        }

    }
}