using _0_Framework.Application;
using _01_PharmaPlusQuery.Contracts.Product;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using ServiceHost.Filters;
using ServiceHost.Services;
using ShopManagement.Application.Contracts.Order;
using ShopManagement.Domain.OrderAgg;
using SiteManagement.Application;
using System.Security.Claims;

namespace ServiceHost.Controllers.Shop
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderApplication _orderApp;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly GetLogoFromSetting _getLogoCommand;

        public OrdersController(IOrderApplication orderApp, IWebHostEnvironment webHostEnvironment, GetLogoFromSetting getLogoCommand)
        {
            _orderApp = orderApp;
            _webHostEnvironment = webHostEnvironment;
            _getLogoCommand = getLogoCommand;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("place")]
        [RateLimit(Limit = 3, Seconds = 60)]
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

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("verify-payment")]
        public async Task<ActionResult<ApiResponse<object>>> VerifyPayment([FromBody] VerifyGatewayPaymentCommand command, CancellationToken cancellation)
        {
            var op = await _orderApp.VerifyGatewayPaymentAsync(command, cancellation);

            return op.IsSucceeded
                ? Ok(ApiResponse<object>.Ok(data: null, message: op.Message))
                : BadRequest(ApiResponse<object>.Fail(op.Message));
        }

        [HttpGet("confirm-callback")]
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

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("download-invoice/{orderId}")]
        [RateLimit(Limit = 5, Seconds = 60)]
        public async Task<ActionResult<ApiResponse<object>>> DownloadInvoice(long orderId, CancellationToken cancellation)
        {
            var nameIdentifier = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? null;
            if (string.IsNullOrWhiteSpace(nameIdentifier))
                BadRequest(ApiResponse<object>
                    .Fail(message: "عملیات غیرمجاز!"));

            long userId = 0;
            if (nameIdentifier != null)
                userId = long.Parse(nameIdentifier);

            if (!await _orderApp.UserOwnsOrder(userId, orderId, cancellation))
                return StatusCode(403,
                    ApiResponse<object>.Fail("شما مجوز دسترسی به این بخش را ندارید."));

            var orderViewModel = await _orderApp.GetDetails(orderId, cancellation);
            if (orderViewModel == null)
                return StatusCode(404,
                    ApiResponse<object>.Fail("موردی یافت نشد."));

            if (!(orderViewModel.Status >= OrderStatus.Paid && orderViewModel.Status <= OrderStatus.Delivered))
                return StatusCode(404,
                    ApiResponse<object>.Fail("موردی یافت نشد."));

            string webRootPath = _webHostEnvironment.WebRootPath;
            string logoPath = await _getLogoCommand.Execute(cancellation);

            var document = new InvoiceDocument(orderViewModel, webRootPath, logoPath);
            byte[] pdfBytes = document.GeneratePdf();

            return File(pdfBytes, "application/pdf", $"Invoice-PharmaPlus-{orderId}.pdf");
        }
    }
}
