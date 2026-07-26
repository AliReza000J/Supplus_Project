using _0_Framework.Application;
using _01_PharmaPlusQuery.Contracts.Comment;
using _01_PharmaPlusQuery.Contracts.Account;
using _01_PharmaPlusQuery.Contracts.Order;
using _01_PharmaPlusQuery.Contracts.Product;
using _01_PharmaPlusQuery.Contracts.Ticket;
using AccountManagement.Application.Contracts.Account;
using AccountManagement.Application.Contracts.Account.ApiDTOs;
using AccountManagement.Application.Contracts.Account.ApplicationDTOs;
using AccountManagement.Application.Contracts.ShippingInfo;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopManagement.Application.Contracts.Order;
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
        private readonly IOrderApplication _orderApp;
        private readonly IShippingInfoApplication _shippingInfoApp;
        private readonly IAccountQuery _accountQuery;
        private readonly IProductQuery _productQuery;
        private readonly IOrderQuery _orderQuery;
        private readonly ITicketQuery _ticketQuery;
        private readonly ICommentQuery _commentQuery;
        private readonly IOtpRateLimiter _otpRateLimiter;
        private readonly IActionRateLimiter _rateLimiter;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/>.
        /// </summary>
        public AccountController(IAccountApplication accountApp,
            IProductQuery productQuery,
            IOrderQuery orderQuery,
            ITicketQuery ticketQuery,
            IShippingInfoApplication shippingInfoApp,
            IOtpRateLimiter otpRateLimiter,
            IActionRateLimiter rateLimiter,
            IAccountQuery accountQuery,
            IOrderApplication orderApp,
            ICommentQuery commentQuery)
        {
            _accountApp = accountApp;
            _productQuery = productQuery;
            _orderQuery = orderQuery;
            _ticketQuery = ticketQuery;
            _shippingInfoApp = shippingInfoApp;
            _otpRateLimiter = otpRateLimiter;
            _rateLimiter = rateLimiter;
            _accountQuery = accountQuery;
            _orderApp = orderApp;
            _commentQuery = commentQuery;
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
        [ProducesResponseType(typeof(ApiResponse<List<FaveProductsCardModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<FaveProductsCardModel>>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<List<FaveProductsCardModel>>>> UserFavoritedProdcts(CancellationToken cancellationToken)
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(idClaim))
                return Unauthorized(ApiResponse<FaveProductsCardModel>.Fail("شناسه کاربر یافت نشد."));

            if (!long.TryParse(idClaim, out var userId))
                return Unauthorized(ApiResponse<FaveProductsCardModel>.Fail($"شناسه کاربری {userId} نامعتبر است."));

            var favs = await _productQuery.GetUserFavoriteList(userId, cancellationToken);

            return Ok(ApiResponse<List<FaveProductsCardModel>>.Ok(
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
        [ProducesResponseType(typeof(ApiResponse<List<OrderSummaryQueryModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<OrderSummaryQueryModel>>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<List<OrderSummaryQueryModel>>>> GetUserOrders(CancellationToken cancellation)
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(idClaim))
                return Unauthorized(ApiResponse<OrderSummaryQueryModel>.Fail("شناسه کاربر یافت نشد."));

            if (!long.TryParse(idClaim, out var userId))
                return Unauthorized(ApiResponse<OrderSummaryQueryModel>.Fail($"شناسه کاربری {userId} نامعتبر است."));

            var orders = await _orderQuery.GetUserOrdersSummaryAsync(userId, cancellation);

            return Ok(ApiResponse<List<OrderSummaryQueryModel>>.Ok(
                orders,
                orders.Any()
                    ? "عملیات با موفقیت انجام شد."
                    : $"هیچ سفارشی یافت نشد!"
            ));
        }

        /// <summary>
        /// Retrieves the details of an order for the authenticated user.
        /// </summary>
        /// <remarks>
        /// Returns details of specific order placed by the currently authenticated user.
        /// </remarks>
        /// <param name="id"></param>
        /// <param name="cancellation">Cancellation token.</param>
        /// <returns>A list of the user's orders.</returns>
        /// <response code="200">Operation successful. User orders list returned (can be empty).</response>
        /// <response code="401">Unauthorized. User ID claim is missing or invalid.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("orders/{id}")]
        [ProducesResponseType(typeof(ApiResponse<OrderQueryModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<OrderQueryModel>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<OrderQueryModel>>> GetOrderDetails(long id, CancellationToken cancellation)
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(idClaim))
                return Unauthorized(ApiResponse<OrderQueryModel>.Fail("شناسه کاربر یافت نشد."));

            if (!long.TryParse(idClaim, out var userId))
                return Unauthorized(ApiResponse<OrderQueryModel>.Fail($"شناسه کاربری {userId} نامعتبر است."));

            var order = await _orderQuery.GetOrderDetailsAsync(id, userId, cancellation);

            return Ok(ApiResponse<OrderQueryModel>.Ok(
                order,
                order != null
                    ? "عملیات با موفقیت انجام شد."
                    : $"سفارش مورد نظر یافت نشد!"
            ));
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("request-return")]
        [ProducesResponseType(typeof(ApiResponse<OperationResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<OperationResult>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<OperationResult>>> RequestReturn([FromBody] RequestReturnDto dto, CancellationToken cancellation)
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(idClaim))
                return Unauthorized(ApiResponse<OrderQueryModel>.Fail("شناسه کاربر یافت نشد."));

            if (!long.TryParse(idClaim, out var userId))
                return Unauthorized(ApiResponse<OrderQueryModel>.Fail($"شناسه کاربری {userId} نامعتبر است."));

            var command = new RequestReturnCommand
            {
                OrderId = dto.OrderId,
                Reason = dto.Reason,
                UserId = userId
            };
            var op = await _orderApp.RequestReturnAsync(command, cancellation);

            return op.IsSucceeded
                ? Ok(ApiResponse<object>.Ok(new(), message: op.Message))
                : BadRequest(ApiResponse<object>.Fail(op.Message));
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
        /// <response code="401">Unauthorized. The user is not authenticated.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("tickets")]
        [ProducesResponseType(typeof(ApiResponse<List<TicketQueryModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<TicketQueryModel>>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<List<TicketQueryModel>>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<List<TicketQueryModel>>>> MyTickets(CancellationToken cancellation)
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(idClaim))
                return Unauthorized(ApiResponse<List<TicketQueryModel>>.Fail("شناسه کاربر یافت نشد."));

            if (!long.TryParse(idClaim, out var userId))
                return Unauthorized(ApiResponse<List<TicketQueryModel>>.Fail($"شناسه کاربری {userId} نامعتبر است."));

            var result = await _ticketQuery.GetUserTickets(userId, cancellation);

            if (result == null || !result.Any())
                return NotFound(ApiResponse<List<TicketQueryModel>>
                    .Fail("موردی یافت نشد."));

            return Ok(ApiResponse<List<TicketQueryModel>>.Ok(result));
        }

        /// <summary>
        /// Tracks a support ticket by its id.
        /// </summary>
        /// <remarks>
        /// Retrieves the details and conversation history of a specific ticket. 
        /// The user must be the owner of the ticket to view its details.
        /// </remarks>
        /// <param name="id">The unique id of the ticket.</param>
        /// <param name="cancellation">A token to cancel the operation.</param>
        /// <response code="200">Success. Returns the details of the ticket.</response>
        /// <response code="404">Not Found. The ticket with the specified code was not found or the user does not have permission to view it.</response>
        /// <response code="401">Unauthorized. The user is not authenticated.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("tickets/{id}")]
        [ProducesResponseType(typeof(ApiResponse<TicketQueryModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<TicketQueryModel>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<TicketQueryModel>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<TicketQueryModel>>> MyTickets(long id, CancellationToken cancellation)
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(idClaim))
                return Unauthorized(ApiResponse<TicketQueryModel>.Fail("شناسه کاربر یافت نشد."));

            if (!long.TryParse(idClaim, out var userId))
                return Unauthorized(ApiResponse<TicketQueryModel>.Fail($"شناسه کاربری {userId} نامعتبر است."));

            var result = await _ticketQuery.GetTicketDetails(id, userId, cancellation);

            if (result == null)
                return NotFound(ApiResponse<TicketQueryModel>
                    .Fail("موردی یافت نشد."));

            return Ok(ApiResponse<TicketQueryModel>.Ok(result));
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("comments")]
        [ProducesResponseType(typeof(ApiResponse<List<UserCommentQueryModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<UserCommentQueryModel>>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<List<UserCommentQueryModel>>>> RequestReturn(CancellationToken cancellation)
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(idClaim))
                return Unauthorized(ApiResponse<List<UserCommentQueryModel>>.Fail("شناسه کاربر یافت نشد."));

            if (!long.TryParse(idClaim, out var userId))
                return Unauthorized(ApiResponse<List<UserCommentQueryModel>>.Fail($"شناسه کاربری {userId} نامعتبر است."));

            var result = await _commentQuery.GetUserComments(userId, cancellation);

            if (result == null || !result.Any())
                return NotFound(ApiResponse<List<UserCommentQueryModel>>
                    .Fail("موردی یافت نشد."));

            return Ok(ApiResponse<List<UserCommentQueryModel>>.Ok(result));
        }

        /// <summary>
        /// Updates the authenticated user's general profile information.
        /// </summary>
        /// <remarks>
        /// Updates details such as fullname, username, email, and profile photo.
        /// The user ID is extracted securely from the JWT token.
        /// This endpoint requires a multipart/form-data payload.
        /// </remarks>
        /// <param name="dto">The updated profile information provided via form data.</param>
        /// <param name="cancellation">Cancellation token.</param>
        /// <returns>An ApiResponse indicating the result of the update operation.</returns>
        /// <response code="200">Operation successful. Profile information updated.</response>
        /// <response code="400">Bad request. Validation failed or update operation was unsuccessful.</response>
        /// <response code="401">Unauthorized. User ID claim is missing or invalid.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("update-profile")]
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


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("set-password")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<object>>> SetPassword(
            [FromForm] SetPasswordApi dto, CancellationToken cancellation)
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(idClaim))
                return Unauthorized(ApiResponse<object>.Fail("شناسه کاربر یافت نشد."));

            if (!long.TryParse(idClaim, out var userId))
                return Unauthorized(ApiResponse<object>.Fail($"شناسه کاربری {userId} نامعتبر است."));

            if (!_rateLimiter.CheckLimit("SetPassword", userId.ToString(), maxRequests: 3, timeWindow: TimeSpan.FromMinutes(60)))
                return StatusCode(StatusCodes.Status429TooManyRequests,
                    ApiResponse<object>.Fail("تعداد درخواست‌های تغییر رمز عبور بیش از حد مجاز است. لطفا بعدا تلاش کنید."));

            var command = new SetPassword
            {
                Id = userId,
                Password = dto.Password,
                RePassword = dto.RePassword,
            };
            var op = await _accountApp.SetPassword(command, userId, cancellation);

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
        /// This endpoint requires a multipart/form-data payload.
        /// </remarks>
        /// <param name="dto">The password change details provided via form data.</param>
        /// <param name="cancellation">Cancellation token.</param>
        /// <returns>An ApiResponse indicating the result of the password change operation.</returns>
        /// <response code="200">Operation successful. Password changed successfully.</response>
        /// <response code="400">Bad request. Validation failed, current password incorrect, or operation unsuccessful.</response>
        /// <response code="401">Unauthorized. User ID claim is missing or invalid.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("change-password")]
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

            if (!_rateLimiter.CheckLimit("ChangePassword", userId.ToString(), maxRequests: 3, timeWindow: TimeSpan.FromMinutes(60)))
                return StatusCode(StatusCodes.Status429TooManyRequests,
                    ApiResponse<object>.Fail("تعداد درخواست‌های تغییر رمز عبور بیش از حد مجاز است. لطفا بعدا تلاش کنید."));

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
            if (!_otpRateLimiter.CanSend(command.Mobile))
                return StatusCode(StatusCodes.Status429TooManyRequests,
                    ApiResponse<object>.Fail("تعداد درخواست‌ها بیش از حد مجاز است."));

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

        // GET: api/Account/ShippingInfos
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("ShippingInfos")]
        public async Task<ActionResult<ApiResponse<List<ShippingInfoViewModel>>>> GetUserAddresses(CancellationToken cancellation)
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(idClaim))
                return Unauthorized(ApiResponse<List<ShippingInfoViewModel>>.Fail("شناسه کاربر یافت نشد."));

            if (!long.TryParse(idClaim, out var userId))
                return Unauthorized(ApiResponse<List<ShippingInfoViewModel>>.Fail($"شناسه کاربری {userId} نامعتبر است."));

            var addresses = await _shippingInfoApp.GetUserAddressesAsync(userId, cancellation);

            return Ok(ApiResponse<List<ShippingInfoViewModel>>.Ok(addresses));
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("ShippingInfos/{id}")]
        public async Task<ActionResult<ApiResponse<EditShippingInfoCommand>>> GetAddressDetails(long id, CancellationToken cancellation)
        {
            var address = await _shippingInfoApp.GetDetailsAsync(id, cancellation);

            if (address == null)
                return NotFound();

            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(idClaim))
                return Unauthorized(ApiResponse<EditShippingInfoCommand>.Fail("شناسه کاربر یافت نشد."));

            if (!long.TryParse(idClaim, out var userId))
                return Unauthorized(ApiResponse<EditShippingInfoCommand>.Fail($"شناسه کاربری {userId} نامعتبر است."));

            if (address.UserId != userId)
                return BadRequest(ApiResponse<EditShippingInfoCommand>.Fail("درخواست نامعتبر است."));

            return Ok(ApiResponse<EditShippingInfoCommand>.Ok(address));
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("ShippingInfos")]
        public async Task<ActionResult<ApiResponse<object>>> CreateAddress([FromBody] CreateShippingInfoDto dto, CancellationToken cancellation)
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(idClaim))
                return Unauthorized(ApiResponse<object>.Fail("شناسه کاربر یافت نشد."));

            if (!long.TryParse(idClaim, out var userId))
                return Unauthorized(ApiResponse<object>.Fail($"شناسه کاربری {userId} نامعتبر است."));

            var command = new CreateShippingInfoCommand
            {
                ReceiverAddress = dto.ReceiverAddress,
                ReceiverFullName = dto.ReceiverFullName,
                ReceiverMobile = dto.ReceiverMobile,
                ReceiverZipCode = dto.ReceiverZipCode,
                UserId = userId
            };

            var result = await _shippingInfoApp.CreateAsync(command, cancellation);

            return result.IsSucceeded
                ? Ok(ApiResponse<object>.Ok(new(), message: "آدرس با موفقیت ثبت شد."))
                : BadRequest(ApiResponse<object>.Fail(result.Message));
        }

        // PUT: api/Account/ShippingInfos
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("ShippingInfos")]
        public async Task<ActionResult<ApiResponse<object>>> EditAddress([FromBody] EditShippingInfoDto dto, CancellationToken cancellation)
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(idClaim))
                return Unauthorized(ApiResponse<object>.Fail("شناسه کاربر یافت نشد."));

            if (!long.TryParse(idClaim, out var userId))
                return Unauthorized(ApiResponse<object>.Fail($"شناسه کاربری {userId} نامعتبر است."));

            var command = new EditShippingInfoCommand
            {
                Id = dto.Id,
                ReceiverAddress = dto.ReceiverAddress,
                ReceiverFullName = dto.ReceiverFullName,
                ReceiverMobile = dto.ReceiverMobile,
                ReceiverZipCode = dto.ReceiverZipCode,
                UserId = userId
            };

            var result = await _shippingInfoApp.EditAsync(command, cancellation);
            return result.IsSucceeded
                ? Ok(ApiResponse<object>.Ok(new(), message: "آدرس با موفقیت ویرایش شد."))
                : BadRequest(ApiResponse<object>.Fail(result.Message));
        }

        // DELETE: api/Account/ShippingInfos
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpDelete("ShippingInfos/{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteAddress(long id, CancellationToken cancellation)
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(idClaim))
                return Unauthorized(ApiResponse<object>.Fail("شناسه کاربر یافت نشد."));

            if (!long.TryParse(idClaim, out var userId))
                return Unauthorized(ApiResponse<object>.Fail($"شناسه کاربری {userId} نامعتبر است."));

            var result = await _shippingInfoApp.DeleteAsync(id, userId, cancellation);
            return result.IsSucceeded
                ? Ok(ApiResponse<object>.Ok(new(), message: "آدرس با موفقیت حذف شد."))
                : BadRequest(ApiResponse<object>.Fail(result.Message));
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("overview")]
        public async Task<ActionResult<ApiResponse<AccountOverviewQueryModel>>> GetUserOverview(CancellationToken cancellation)
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(idClaim))
                return Unauthorized(ApiResponse<AccountOverviewQueryModel>.Fail("شناسه کاربر یافت نشد."));

            if (!long.TryParse(idClaim, out var userId))
                return Unauthorized(ApiResponse<AccountOverviewQueryModel>.Fail($"شناسه کاربری {userId} نامعتبر است."));

            var overview = await _accountQuery.GetUserOverview(userId, cancellation);

            return Ok(ApiResponse<AccountOverviewQueryModel>.Ok(overview));
        }
    }
}