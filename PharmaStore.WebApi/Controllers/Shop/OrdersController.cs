using _0_Framework.Application;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using ShopManagement.Application.Contracts.Order;
using ShopManagement.Domain.OrderAgg;
using SiteManagement.Application;
using System.Security.Claims;
using PharmaShared.Web.Filters;
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
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly GetLogoFromSetting _getLogoCommand;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrdersController"/> class.
        /// </summary>
        /// <param name="orderApp">The application service for managing orders.</param>
        /// <param name="webHostEnvironment">The environment information for retrieving web root paths.</param>
        /// <param name="getLogoCommand">The command to retrieve the application's logo for invoice generation.</param>
        public OrdersController(IOrderApplication orderApp, IWebHostEnvironment webHostEnvironment, GetLogoFromSetting getLogoCommand)
        {
            _orderApp = orderApp;
            _webHostEnvironment = webHostEnvironment;
            _getLogoCommand = getLogoCommand;
        }

        /// <summary>
        /// Places a new order for the authenticated user.
        /// </summary>
        /// <remarks>
        /// This endpoint processes the current cart and places an order. 
        /// It accepts form data to support uploading payment receipts for offline/transfer payment methods.
        /// Rate limit: 3 requests per 60 seconds.
        /// </remarks>
        /// <param name="orderInput">The order details and shipping information (DTO to be documented later).</param>
        /// <param name="cancellation">A token to cancel the operation.</param>
        /// <response code="200">Success. Returns the result of the order placement.</response>
        /// <response code="400">Bad Request. The order placement failed.</response>
        /// <response code="401">Unauthorized. The user is not authenticated or could not be identified.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("place")]
        [RateLimit(Limit = 3, Seconds = 60)]
        [ProducesResponseType(typeof(ApiResponse<PlaceOrderResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PlaceOrderResult>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<PlaceOrderResult>>> PlaceOrder([FromForm] CreateOrderCommandDto orderInput, CancellationToken cancellation)
        {
            var nameIdentifier = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var command = new CreateOrderCommand
            {
                CardOwnerName = orderInput.CardOwnerName,
                NationalCode = orderInput.NationalCode,
                PaymentReceiptPic = orderInput.PaymentReceiptPic,
                PayMethod = orderInput.PayMethod,
                ReceiverAddress = orderInput.ReceiverAddress,
                ReceiverFullName = orderInput.ReceiverFullName,
                CouponCode = orderInput.CouponCode,
                ReceiverMobile = orderInput.ReceiverMobile,
                ReceiverZipCode = orderInput.ReceiverZipCode
            };
            if (long.TryParse(nameIdentifier, out long userId))
            {
                command.UserId = userId;
            }
            else
            {
                return Unauthorized(ApiResponse<PlaceOrderResult>.Fail("کاربر شناسایی نشد."));
            }

            var (result, op) = await _orderApp.PlaceOrderAsync(command, cancellation);

            if (op.IsSucceeded && result != null)
            {
                return Ok(ApiResponse<PlaceOrderResult>.Ok(data: result, message: op.Message));
            }
            else
            {
                return BadRequest(ApiResponse<PlaceOrderResult>.Fail(op.Message));
            }
        }

        /// <summary>
        /// Verifies a gateway payment after returning from the bank.
        /// </summary>
        /// <remarks>
        /// Call this endpoint using the track ID received from the callback to finalize the payment in the system.
        /// </remarks>
        /// <param name="command">The verification details (DTO to be documented later).</param>
        /// <param name="cancellation">A token to cancel the operation.</param>
        /// <response code="200">Success. The payment was successfully verified.</response>
        /// <response code="400">Bad Request. Verification failed.</response>
        /// <response code="401">Unauthorized. The user is not authenticated.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("verify-payment")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<object>>> VerifyPayment([FromBody] VerifyGatewayPaymentCommand command, CancellationToken cancellation)
        {
            var op = await _orderApp.VerifyGatewayPaymentAsync(command, cancellation);

            return op.IsSucceeded
                ? Ok(ApiResponse<object>.Ok(data: null, message: op.Message))
                : BadRequest(ApiResponse<object>.Fail(op.Message));
        }

        /// <summary>
        /// Handles the callback from the payment gateway.
        /// </summary>
        /// <remarks>
        /// The payment gateway redirects the user to this endpoint after attempting a payment. 
        /// It returns a tracking ID and the status of the gateway operation. 
        /// If successful, the track ID should be used in the `verify-payment` endpoint.
        /// </remarks>
        /// <param name="trackId">The unique tracking ID assigned to the transaction by the gateway.</param>
        /// <param name="success">Indicates whether the gateway reported a success (1) or failure.</param>
        /// <param name="status">The status code returned by the payment gateway.</param>
        /// <response code="200">The gateway operation was successful. Verification is now required.</response>
        /// <response code="400">The gateway operation failed or was canceled by the user.</response>
        [HttpGet("confirm-callback")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult ConfirmCallback([FromQuery] long trackId, [FromQuery] int success, [FromQuery] int status)
        {
            if (success == 1)
            {
                return Ok(new
                {
                    Message = "پرداخت در درگاه موفق بود. حالا باید عملیات Verify را انجام دهید.",
                    Instruction = "مقدار trackId زیر را کپی کرده و در متد verify-payment در سواگر وارد کنید.",
                    TrackId = trackId,
                    Status = status
                });
            }

            return BadRequest(new
            {
                Message = "پرداخت در درگاه شکست خورد یا لغو شد.",
                TrackId = trackId,
                Status = status
            });
        }

        /// <summary>
        /// Downloads a PDF invoice for a specific order.
        /// </summary>
        /// <remarks>
        /// The authenticated user can only download invoices for their own orders. 
        /// The order must be in a valid status (Paid up to Delivered).
        /// Rate limit: 5 requests per 60 seconds.
        /// </remarks>
        /// <param name="orderId">The unique identifier of the order.</param>
        /// <param name="cancellation">A token to cancel the operation.</param>
        /// <response code="200">Returns the PDF file of the invoice.</response>
        /// <response code="400">Bad Request. Invalid user ID or missing claims.</response>
        /// <response code="401">Unauthorized. The user is not authenticated.</response>
        /// <response code="403">Forbidden. The user does not own the requested order.</response>
        /// <response code="404">Not Found. The order does not exist or is not in a valid state for an invoice.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("download-invoice/{orderId}")]
        [RateLimit(Limit = 5, Seconds = 60)]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> DownloadInvoice(long orderId, CancellationToken cancellation)
        {
            var nameIdentifier = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? null;
            if (string.IsNullOrWhiteSpace(nameIdentifier))
                return BadRequest(ApiResponse<object>.Fail(message: "عملیات غیرمجاز!"));

            long userId = 0;
            if (nameIdentifier != null)
                userId = long.Parse(nameIdentifier);

            if (!await _orderApp.UserOwnsOrder(userId, orderId, cancellation))
                return StatusCode(403, ApiResponse<object>.Fail("شما مجوز دسترسی به این بخش را ندارید."));

            var orderViewModel = await _orderApp.GetDetails(orderId, cancellation);
            if (orderViewModel == null)
                return StatusCode(404, ApiResponse<object>.Fail("موردی یافت نشد."));

            if (!(orderViewModel.Status >= OrderStatus.Paid && orderViewModel.Status <= OrderStatus.Delivered))
                return StatusCode(404, ApiResponse<object>.Fail("موردی یافت نشد."));

            string webRootPath = _webHostEnvironment.WebRootPath;
            string logoPath = await _getLogoCommand.Execute(cancellation);

            var document = new InvoiceDocument(orderViewModel, webRootPath, logoPath);
            byte[] pdfBytes = document.GeneratePdf();

            return File(pdfBytes, "application/pdf", $"Invoice-PharmaPlus-{orderId}.pdf");
        }
    }
}
