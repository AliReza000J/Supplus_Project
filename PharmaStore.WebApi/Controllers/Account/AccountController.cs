using _0_Framework.Application;
using _01_PharmaPlusQuery.Contracts.Order;
using _01_PharmaPlusQuery.Contracts.Product;
using _01_PharmaPlusQuery.Contracts.Ticket;
using AccountManagement.Application.Contracts.Account;
using AccountManagement.Application.Contracts.Account.ApiDTOs;
using AccountManagement.Application.Contracts.Account.ApplicationDTOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaShared.Web.Filters;
using System.Security.Claims;

namespace PharmaStore.WebApi.Controllers.Account
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
        private readonly ITicketQuery _ticketQuery;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/>.
        /// </summary>
        public AccountController(IAccountApplication accountApp,
            IProductQuery productQuery,
            IOrderQuery orderQuery,
            ITicketQuery ticketQuery)
        {
            _accountApp = accountApp;
            _productQuery = productQuery;
            _orderQuery = orderQuery;
            _ticketQuery = ticketQuery;
        }

        /// <summary>
        /// Retrieves the profile details of the currently authenticated user.
        /// </summary>
        /// <remarks>
        /// Extracts the user ID from the JWT token claims and returns the corresponding account information.
        /// </remarks>
        /// <param name="cancellation">Cancellation token.</param>
        /// <returns>The account details of the authenticated user.</returns>
        /// <response code="200">Operation successful. User details returned.</response>
        /// <response code="401">Unauthorized. User ID claim is missing or invalid.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("me")]
        [ProducesResponseType(typeof(ApiResponse<AccountViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<AccountViewModel>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<AccountViewModel>>> Me(CancellationToken cancellation)
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(idClaim))
                return Unauthorized(ApiResponse<AccountViewModel>.Fail("شناسه کاربر یافت نشد."));

            if (!long.TryParse(idClaim, out var userId))
                return Unauthorized(ApiResponse<AccountViewModel>.Fail($"شناسه کاربری {userId} نامعتبر است."));

            var model = await _accountApp.GetAccountDetails(userId, cancellation);

            return Ok(ApiResponse<AccountViewModel>.Ok(model, "اطلاعات با موفقیت بازیابی شد."));
        }

        /// <summary>
        /// Retrieves a list of the authenticated user's favorite products.
        /// </summary>
        /// <remarks>
        /// Returns the products that the current user has added to their favorites list.
        /// </remarks>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of favorite products.</returns>
        /// <response code="200">Operation successful. Favorite products list returned (can be empty).</response>
        /// <response code="401">Unauthorized. User ID claim is missing or invalid.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("favorites")]
        [ProducesResponseType(typeof(ApiResponse<List<ProductQueryModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<ProductQueryModel>>), StatusCodes.Status401Unauthorized)]
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

        /// <summary>
        /// Retrieves the order history for the authenticated user.
        /// </summary>
        /// <remarks>
        /// Returns a list of all previous orders placed by the currently authenticated user.
        /// </remarks>
        /// <param name="cancellation">Cancellation token.</param>
        /// <returns>A list of the user's orders.</returns>
        /// <response code="200">Operation successful. User orders list returned (can be empty).</response>
        /// <response code="401">Unauthorized. User ID claim is missing or invalid.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("orders")]
        [ProducesResponseType(typeof(ApiResponse<List<OrderQueryModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<OrderQueryModel>>), StatusCodes.Status401Unauthorized)]
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
                    : $"هیچ محصولی در لیست سفارشات پیشین شما یافت نشد!"
            ));
        }

        /// <summary>
        /// Retrieves the support tickets created by the authenticated user.
        /// </summary>
        /// <remarks>
        /// Returns a list of all tickets associated with the current user's identifier.
        /// </remarks>
        /// <param name="cancellation">Cancellation token.</param>
        /// <returns>A list of user's support tickets.</returns>
        /// <response code="200">Operation successful. Tickets list returned.</response>
        /// <response code="404">Not found. The user has no tickets.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("tickets")]
        [ProducesResponseType(typeof(ApiResponse<List<TicketQueryModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<TicketQueryModel>>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<List<TicketQueryModel>>>> MyTickets(CancellationToken cancellation)
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

            var result = await _ticketQuery.GetUserTickets(idClaim, cancellation);

            if (result == null || !result.Any())
                return NotFound(ApiResponse<List<TicketQueryModel>>
                    .Fail("موردی یافت نشد."));

            return Ok(ApiResponse<List<TicketQueryModel>>.Ok(result));
        }

        /// <summary>
        /// Updates the authenticated user's general profile information.
        /// </summary>
        /// <remarks>
        /// Updates details such as fullname, username, email, and profile photo.
        /// The user ID is extracted securely from the JWT token.
        /// This endpoint is rate-limited to 5 requests per 60 seconds and requires a multipart/form-data payload.
        /// </remarks>
        /// <param name="dto">The updated profile information provided via form data.</param>
        /// <param name="cancellation">Cancellation token.</param>
        /// <returns>An ApiResponse indicating the result of the update operation.</returns>
        /// <response code="200">Operation successful. Profile information updated.</response>
        /// <response code="400">Bad request. Validation failed or update operation was unsuccessful.</response>
        /// <response code="401">Unauthorized. User ID claim is missing or invalid.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("update-profile")]
        [RateLimit(Limit = 5, Seconds = 60)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<object>>> EditProfile(
            [FromForm] UpdateGeneralInfoDto dto, CancellationToken cancellation)
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(idClaim))
                return Unauthorized(ApiResponse<object>.Fail("شناسه کاربر یافت نشد."));

            if (!long.TryParse(idClaim, out var userId))
                return Unauthorized(ApiResponse<object>.Fail($"شناسه کاربری {userId} نامعتبر است."));

            var command = new UpdateGeneralInfo
            {
                Email = dto.Email,
                Id = userId,
                Fullname = dto.Fullname,
                Username = dto.Username,
                ProfilePhoto = dto.ProfilePhoto
            };

            var op = await _accountApp.UpdateProfileInfo(command, cancellation);

            return op.IsSucceeded
                ? Ok(ApiResponse<object>.Ok(new(), message: op.Message))
                : BadRequest(ApiResponse<object>.Fail(op.Message));
        }

        /// <summary>
        /// Changes the authenticated user's password.
        /// </summary>
        /// <remarks>
        /// Validates the current password and updates it to the new password.
        /// The user ID is extracted securely from the JWT token.
        /// This endpoint is rate-limited to 3 requests per 60 seconds and requires a multipart/form-data payload.
        /// </remarks>
        /// <param name="dto">The password change details provided via form data.</param>
        /// <param name="cancellation">Cancellation token.</param>
        /// <returns>An ApiResponse indicating the result of the password change operation.</returns>
        /// <response code="200">Operation successful. Password changed successfully.</response>
        /// <response code="400">Bad request. Validation failed, current password incorrect, or operation unsuccessful.</response>
        /// <response code="401">Unauthorized. User ID claim is missing or invalid.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("change-password")]
        [RateLimit(Limit = 3, Seconds = 60)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<object>>> ChangePassword(
            [FromForm] ChangePasswordApi dto, CancellationToken cancellation)
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(idClaim))
                return Unauthorized(ApiResponse<object>.Fail("شناسه کاربر یافت نشد."));

            if (!long.TryParse(idClaim, out var userId))
                return Unauthorized(ApiResponse<object>.Fail($"شناسه کاربری {userId} نامعتبر است."));

            var command = new ChangePassword
            {
                Id = userId,
                Password = dto.Password,
                RePassword = dto.RePassword,
                CurrentPassword = dto.CurrentPassword
            };
            var op = await _accountApp.ChangePassword(command, userId, cancellation);

            return op.IsSucceeded
                ? Ok(ApiResponse<object>.Ok(new(), message: op.Message))
                : BadRequest(ApiResponse<object>.Fail(op.Message));
        }

        /// <summary>
        /// Requests an OTP to change the authenticated user's mobile number.
        /// </summary>
        /// <remarks>
        /// Extracts the user ID securely from the JWT token and initiates an OTP request 
        /// for the new mobile number provided in the request body.
        /// </remarks>
        /// <param name="command">The data transfer object containing the new mobile number.</param>
        /// <param name="cancellation">Cancellation token.</param>
        /// <returns>An action result indicating the success or failure of the OTP request.</returns>
        /// <response code="200">Operation successful. OTP sent successfully.</response>
        /// <response code="400">Bad request. Validation failed or OTP request was unsuccessful.</response>
        /// <response code="401">Unauthorized. User ID claim is missing or invalid.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("change-mobile/request")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<object>>> RequestMobileChange([FromBody] SendOtpDto command, CancellationToken cancellation)
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(idClaim))
                return Unauthorized(ApiResponse<object>.Fail("شناسه کاربر یافت نشد."));

            if (!long.TryParse(idClaim, out var userId))
                return Unauthorized(ApiResponse<object>.Fail($"شناسه کاربری {userId} نامعتبر است."));

            var result = await _accountApp.RequestMobileChangeOtp(userId, command.Mobile, cancellation);

            return result.IsSucceeded
                ? Ok(ApiResponse<object>.Ok(new(), message: result.Message))
                : BadRequest(ApiResponse<object>.Fail(result.Message));
        }

        /// <summary>
        /// Verifies the OTP and updates the authenticated user's mobile number.
        /// </summary>
        /// <remarks>
        /// This endpoint validates the provided verification code against the requested new mobile number.
        /// The user ID is extracted securely from the JWT token.
        /// If successful, the user's mobile number is updated in the system.
        /// </remarks>
        /// <param name="dto">The payload containing new mobile number, and the verification code.</param>
        /// <param name="cancellation">A token to monitor for cancellation requests.</param>
        /// <returns>Returns a success message if the mobile number is updated, or an error message if verification fails.</returns>
        /// <response code="200">Operation successful. Mobile number updated successfully.</response>
        /// <response code="400">Bad request. Validation failed, or the verification code is invalid/expired.</response>
        /// <response code="401">Unauthorized. User is not authenticated.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("change-mobile/verify")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<object>>> VerifyAndChangeMobile([FromBody] VerifyOtpDto dto, CancellationToken cancellation)
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(idClaim))
                return Unauthorized(ApiResponse<object>.Fail("شناسه کاربر یافت نشد."));

            if (!long.TryParse(idClaim, out var userId))
                return Unauthorized(ApiResponse<object>.Fail($"شناسه کاربری {userId} نامعتبر است."));

            var command = new ChangeMobileDto
            {
                Id = userId,
                Mobile = dto.Mobile,
                Code = dto.Code
            };

            var result = await _accountApp.ChangeMobileApi(command, cancellation);

            return result.IsSucceeded
                ? Ok(ApiResponse<object>.Ok(new(), message: result.Message))
                : BadRequest(ApiResponse<object>.Fail(result.Message));
        }

    }
}