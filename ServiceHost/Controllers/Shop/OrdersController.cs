using _0_Framework.Application;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using ServiceHost.Services;
using ShopManagement.Application.Contracts.Order;
using ShopManagement.Domain.OrderAgg;
using System.Security.Claims;

namespace ServiceHost.Controllers.Shop
{
    [Route("shop/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderApplication _orderApp;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public OrdersController(IOrderApplication orderApp, IWebHostEnvironment webHostEnvironment)
        {
            _orderApp = orderApp;
            _webHostEnvironment = webHostEnvironment;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("place")]
        public async Task<ActionResult<ApiResponse<PlaceOrderResult>>> PlaceOrder([FromBody] CreateOrderCommand command)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (long.TryParse(userId, out long accountId))
            {
                command.AccountId = accountId;
            }
            else
            {
                return Unauthorized(ApiResponse<PlaceOrderResult>.Fail("کاربر شناسایی نشد."));
            }

            var (result, op) = await _orderApp.PlaceOrderAsync(command);

            if (op.IsSucceeded && result != null)
            {
                // موفقیت: چه کارت به کارت باشد (بدون ریدارکت) چه درگاه (با ریدارکت)
                // فرانت‌اند بر اساس result.ShouldRedirect تصمیم می‌گیرد
                return Ok(ApiResponse<PlaceOrderResult>.Ok(data: result, message: op.Message));
            }
            else
            {
                return BadRequest(ApiResponse<PlaceOrderResult>.Fail(op.Message));
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("verify-payment")]
        public async Task<ActionResult<ApiResponse<object>>> VerifyPayment([FromBody] VerifyGatewayPaymentCommand command)
        {
            // این متد زمانی صدا زده می‌شود که کاربر از درگاه به سایت شما برگشته
            // و فرانت‌اند پارامترهای Authority و RefId را به این متد می‌فرستد.

            var op = await _orderApp.VerifyGatewayPaymentAsync(command);

            return op.IsSucceeded
                ? Ok(ApiResponse<object>.Ok(data: null, message: op.Message)) // موفق و سبد خالی شده
                : BadRequest(ApiResponse<object>.Fail(op.Message)); // ناموفق، سبد پر مانده
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
        public async Task<ActionResult<ApiResponse<object>>> DownloadInvoice(long orderId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0";
            if (!await _orderApp.UserOwnsOrder(userId, orderId))
                return StatusCode(403,
                    ApiResponse<object>.Fail("شما مجوز دسترسی به این بخش را ندارید."));

            var orderViewModel = await _orderApp.GetDetails(orderId);
            if (orderViewModel == null)
                return StatusCode(404,
                    ApiResponse<object>.Fail("موردی یافت نشد."));

            if (!(orderViewModel.Status >= OrderStatus.Paid && orderViewModel.Status <= OrderStatus.Delivered))
                return StatusCode(404,
                    ApiResponse<object>.Fail("موردی یافت نشد."));

            string webRootPath = _webHostEnvironment.WebRootPath;

            var document = new InvoiceDocument(orderViewModel, webRootPath);
            byte[] pdfBytes = document.GeneratePdf();

            return File(pdfBytes, "application/pdf", $"Invoice-PharmaPlus-{orderId}.pdf");
        }
    }
}
