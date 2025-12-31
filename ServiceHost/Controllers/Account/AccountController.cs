using _0_Framework.Application;
using _01_PharmaPlusQuery.Contracts.Order;
using _01_PharmaPlusQuery.Contracts.Product;
using AccountManagement.Application.Contracts.Account;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceHost.Controllers.Auth;
using System.Security.Claims;

namespace ServiceHost.Controllers.Account
{
    [Route("account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountApplication _accountApp;
        private readonly IProductQuery _productQuery;
        private readonly IOrderQuery _orderQuery;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/>.
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
        // ME (Current user profile)
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
        public async Task<ActionResult<ApiResponse<AccountViewModel>>> Me()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(idClaim))
                return Unauthorized(ApiResponse<AccountViewModel>.Fail("شناسه کاربر یافت نشد."));

            if (!long.TryParse(idClaim, out var accountId))
                return Unauthorized(ApiResponse<AccountViewModel>.Fail($"شناسه کاربری {accountId} نامعتبر است."));

            var model = await _accountApp.GetAccountBy(accountId);

            return Ok(ApiResponse<AccountViewModel>.Ok(model, "اطلاعات با موفقیت بازیابی شد."));
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("favorites")]
        public async Task<ActionResult<ApiResponse<List<ProductQueryModel>>>> UserFavoritedProdcts(CancellationToken cancellationToken)
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(idClaim))
                return Unauthorized(ApiResponse<ProductQueryModel>.Fail("شناسه کاربر یافت نشد."));

            if (!long.TryParse(idClaim, out var accountId))
                return Unauthorized(ApiResponse<ProductQueryModel>.Fail($"شناسه کاربری {accountId} نامعتبر است."));

            var favs = await _productQuery.GetUserFavoriteList(idClaim, cancellationToken);

            return Ok(ApiResponse<List<ProductQueryModel>>.Ok(
                favs,
                favs.Any()
                    ? "عملیات با موفقیت انجام شد."
                    : $"هیچ محصولی در لیست علاقه‌مندی شما یافت نشد!."
            ));
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("orders")]
        public async Task<ActionResult<ApiResponse<List<OrderQueryModel>>>> GetUserOrders(CancellationToken cancellation)
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(idClaim))
                return Unauthorized(ApiResponse<OrderQueryModel>.Fail("شناسه کاربر یافت نشد."));

            if (!long.TryParse(idClaim, out var accountId))
                return Unauthorized(ApiResponse<OrderQueryModel>.Fail($"شناسه کاربری {accountId} نامعتبر است."));

            var orders = await _orderQuery.GetOrdersByUser(accountId, cancellation);

            return Ok(ApiResponse<List<OrderQueryModel>>.Ok(
                orders,
                orders.Any()
                    ? "عملیات با موفقیت انجام شد."
                    : $"هیچ محصولی در لیست سفارشات پیشین شما یافت نشد!."
            ));
        }
    }
}