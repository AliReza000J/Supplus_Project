using _0_Framework.Application;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using ShopManagement.Application.Contracts.Order;
using ShopManagement.Domain.OrderAgg;
using SiteManagement.Application;
using System.Security.Claims;
using PharmaShared.Web.Services;

namespace PharmaStore.WebApi.Controllers.Shop
{
    /// <summary>
    /// Manages customer orders, including order placement, payment verification, callbacks, and generating invoices.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderApplication _orderApp;
        private readonly IConfiguration _config;
        private readonly GetLogoFromSetting _getLogoCommand;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrdersController"/> class.
        /// </summary>
        /// <param name="orderApp">The application service for managing orders.</param>
        /// <param name="config">The environment information for retrieving web root paths.</param>
        /// <param name="getLogoCommand">The command to retrieve the application's logo for invoice generation.</param>
        public OrdersController(IOrderApplication orderApp, IConfiguration config, GetLogoFromSetting getLogoCommand)
        {
            _orderApp = orderApp;
            _config = config;
            _getLogoCommand = getLogoCommand;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("checkout-preview")]
        [ProducesResponseType(typeof(ApiResponse<CheckoutPreviewDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<CheckoutPreviewDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<CheckoutPreviewDto>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<CheckoutPreviewDto>>> GetCheckoutPreview([FromQuery] string? couponCode, [FromServices] IActionRateLimiter rateLimiter, CancellationToken cancellationToken)
        {
            var nameIdentifier = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(nameIdentifier, out long userId))
                return Unauthorized(ApiResponse<CheckoutPreviewDto>.Fail("کاربر شناسایی نشد."));

            if (!rateLimiter.CheckLimit("CheckoutPreview", userId.ToString(), maxRequests: 20, timeWindow: TimeSpan.FromMinutes(1)))
            {
                return StatusCode(StatusCodes.Status429TooManyRequests,
                    ApiResponse<CheckoutPreviewDto>.Fail("تعداد درخواست‌ها بیش از حد مجاز است."));
            }

            var (previewDto, operationResult) = await _orderApp.GetCheckoutPreviewAsync(userId, couponCode, cancellationToken);

            if (!operationResult.IsSucceeded || previewDto == null)
            {
                return BadRequest(ApiResponse<CheckoutPreviewDto>.Fail(operationResult.Message));
            }

            return Ok(ApiResponse<CheckoutPreviewDto>.Ok(previewDto));
        }


        /// <summary>
        /// Places a new order for the authenticated user.
        /// </summary>
        /// <remarks>
        /// This endpoint processes the current cart and places an order. 
        /// It accepts form data to support uploading payment receipts for offline/transfer payment methods.
        /// </remarks>
        /// <param name="orderInput">The order details and shipping information (DTO to be documented later).</param>
        /// <param name="rateLimiter"></param>
        /// <param name="cancellation">A token to cancel the operation.</param>
        /// <response code="200">Success. Returns the result of the order placement.</response>
        /// <response code="400">Bad Request. The order placement failed.</response>
        /// <response code="401">Unauthorized. The user is not authenticated or could not be identified.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("place")]
        [ProducesResponseType(typeof(ApiResponse<CreateOrderResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<CreateOrderResult>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<CreateOrderResult>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<CreateOrderResult>>> PlaceOrder([FromForm] CreateOrderCommandDto orderInput, [FromServices] IActionRateLimiter rateLimiter, CancellationToken cancellation)
        {
            var nameIdentifier = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!long.TryParse(nameIdentifier, out long userId))
            {
                return Unauthorized(ApiResponse<CreateOrderResult>.Fail("کاربر شناسایی نشد."));
            }

            if (!rateLimiter.CheckLimit("PlaceOrder", userId.ToString(), maxRequests: 3, timeWindow: TimeSpan.FromMinutes(5)))
            {
                return StatusCode(StatusCodes.Status429TooManyRequests,
                    ApiResponse<CreateOrderResult>.Fail("شما به تازگی یک سفارش ثبت کرده‌اید. لطفا چند دقیقه بعد تلاش کنید."));
            }

            var command = new CreateOrderCommand
            {
                CardOwnerName = orderInput.CardOwnerName,
                NationalCode = orderInput.NationalCode,
                PaymentReceiptPic = orderInput.PaymentReceiptPic,
                PayMethod = orderInput.PayMethod,
                CouponCode = orderInput.CouponCode,
                ShippingInfoId = orderInput.ShippingInfoId,
                UserId = userId
            };

            var (result, op) = await _orderApp.CreateOrderAsync(command, cancellation);

            if (op.IsSucceeded && result != null)
            {
                return Ok(ApiResponse<CreateOrderResult>.Ok(data: result, message: op.Message));
            }
            else
            {
                return BadRequest(ApiResponse<CreateOrderResult>.Fail(op.Message));
            }
        }

        /// <summary>
        /// Verifies a gateway payment after returning from the bank.
        /// </summary>
        /// <remarks>
        /// Call this endpoint using the track ID received from the callback to finalize the payment in the system.
        /// </remarks>
        /// <param name="command">The verification details (DTO to be documented later).</param>
        /// <param name="rateLimiter"></param>
        /// <param name="cancellation">A token to cancel the operation.</param>
        /// <response code="200">Success. The payment was successfully verified.</response>
        /// <response code="400">Bad Request. Verification failed.</response>
        /// <response code="401">Unauthorized. The user is not authenticated.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("verify-payment")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<object>>> VerifyPayment([FromBody] VerifyGatewayPaymentCommand command, [FromServices] IActionRateLimiter rateLimiter, CancellationToken cancellation)
        {
            var nameIdentifier = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(nameIdentifier, out long userId))
                return Unauthorized(ApiResponse<object>.Fail("کاربر شناسایی نشد."));

            if (!rateLimiter.CheckLimit("VerifyPayment", userId.ToString(), maxRequests: 5, timeWindow: TimeSpan.FromMinutes(10)))
            {
                return StatusCode(StatusCodes.Status429TooManyRequests,
                    ApiResponse<object>.Fail("تعداد درخواست‌های تایید پرداخت بیش از حد مجاز است."));
            }

            var op = await _orderApp.VerifyGatewayPaymentAsync(command, cancellation);

            return op.IsSucceeded
                ? Ok(ApiResponse<object>.Ok(data: new(), message: op.Message))
                : BadRequest(ApiResponse<object>.Fail(op.Message));
        }

        ///// <summary>
        ///// Handles the callback from the payment gateway.
        ///// </summary>
        ///// <remarks>
        ///// The payment gateway redirects the user to this endpoint after attempting a payment. 
        ///// It returns a tracking ID and the status of the gateway operation. 
        ///// If successful, the track ID should be used in the `verify-payment` endpoint.
        ///// </remarks>
        ///// <param name="trackId">The unique tracking ID assigned to the transaction by the gateway.</param>
        ///// <param name="success">Indicates whether the gateway reported a success (1) or failure.</param>
        ///// <param name="status">The status code returned by the payment gateway.</param>
        ///// <response code="200">The gateway operation was successful. Verification is now required.</response>
        ///// <response code="400">The gateway operation failed or was canceled by the user.</response>
        //[HttpGet("confirm-callback")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //public IActionResult ConfirmCallback([FromQuery] long trackId, [FromQuery] int success, [FromQuery] int status)
        //{
        //    if (success == 1)
        //    {
        //        return Ok(new
        //        {
        //            Message = "پرداخت در درگاه موفق بود. حالا باید عملیات Verify را انجام دهید.",
        //            Instruction = "مقدار trackId زیر را کپی کرده و در متد verify-payment در سواگر وارد کنید.",
        //            TrackId = trackId,
        //            Status = status
        //        });
        //    }

        //    return BadRequest(new
        //    {
        //        Message = "پرداخت در درگاه شکست خورد یا لغو شد.",
        //        TrackId = trackId,
        //        Status = status
        //    });
        //}

        /// <summary>
        /// Downloads a PDF invoice for a specific order.
        /// </summary>
        /// <remarks>
        /// The authenticated user can only download invoices for their own orders. 
        /// The order must be in a valid status (Paid up to Delivered).
        /// </remarks>
        /// <param name="orderId">The unique identifier of the order.</param>
        /// <param name="rateLimiter"></param>
        /// <param name="cancellation">A token to cancel the operation.</param>
        /// <response code="200">Returns the PDF file of the invoice.</response>
        /// <response code="400">Bad Request. Invalid user ID or missing claims.</response>
        /// <response code="401">Unauthorized. The user is not authenticated.</response>
        /// <response code="403">Forbidden. The user does not own the requested order.</response>
        /// <response code="404">Not Found. The order does not exist or is not in a valid state for an invoice.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("download-invoice/{orderId}")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> DownloadInvoice(long orderId, [FromServices] IActionRateLimiter rateLimiter, CancellationToken cancellation)
        {
            var nameIdentifier = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? null;
            if (string.IsNullOrWhiteSpace(nameIdentifier))
                return BadRequest(ApiResponse<object>.Fail(message: "عملیات غیرمجاز!"));

            long userId = 0;
            if (nameIdentifier != null)
                userId = long.Parse(nameIdentifier);

            if (!rateLimiter.CheckLimit("DownloadInvoice", userId.ToString(), maxRequests: 10, timeWindow: TimeSpan.FromMinutes(5)))
            {
                return StatusCode(StatusCodes.Status429TooManyRequests,
                    ApiResponse<object>.Fail("تعداد درخواست‌های دانلود فاکتور بیش از حد مجاز است."));
            }

            if (!await _orderApp.UserOwnsOrder(userId, orderId, cancellation))
                return StatusCode(403, ApiResponse<object>.Fail("شما مجوز دسترسی به این بخش را ندارید."));

            var orderViewModel = await _orderApp.GetDetails(orderId, cancellation);
            if (orderViewModel == null)
                return StatusCode(404, ApiResponse<object>.Fail("موردی یافت نشد."));

            if (!(orderViewModel.Status >= OrderStatus.Paid && orderViewModel.Status <= OrderStatus.Delivered))
                return StatusCode(404, ApiResponse<object>.Fail("موردی یافت نشد."));

            string logoPath = await _getLogoCommand.Execute(cancellation);

            var document = new InvoiceDocument(orderViewModel, _config, logoPath);
            byte[] pdfBytes = document.GeneratePdf();

            return File(pdfBytes, "application/pdf", $"Invoice-PharmaPlus-{orderId}.pdf");
        }
    }
}
