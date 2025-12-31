using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using ShopManagement.Application.Contracts.Order;
using _0_Framework.Application;
using Hangfire;
using ServiceHost.Services;

namespace ServiceHost.Areas.Admin.Pages.Shop.Orders
{
    public class DetailsModel : PageModel
    {
        [TempData]
        public string Message { get; set; }

        public OrderViewModel Order { get; set; }

        [BindProperty]
        public RejectPaymentManually RejectPayment { get; set; }

        [BindProperty]
        public ConfirmPaymentManually ConfirmPayment { get; set; }

        [BindProperty]
        public ShipCommand Ship { get; set; }

        private readonly IOrderApplication _orderApplication;
        private readonly IAuthHelper _authHelper;
        private readonly IAdminSmsRateLimiter _adminSmsRateLimiter;
        public DetailsModel(IOrderApplication orderApplication, IAuthHelper authHelper, IAdminSmsRateLimiter adminSmsRateLimiter)
        {
            _orderApplication = orderApplication;
            _authHelper = authHelper;
            _adminSmsRateLimiter = adminSmsRateLimiter;
        }

        public async Task<IActionResult> OnGet(long id)
        {
            Order = await _orderApplication.GetDetails(id);

            if (Order == null)
                return NotFound();

            RejectPayment = new RejectPaymentManually { OrderId = id };
            ConfirmPayment = new ConfirmPaymentManually { OrderId = id };
            Ship = new ShipCommand { OrderId = id, Mobile = Order.RecieverMobile ?? "" };

            return Page();
        }

        public async Task<IActionResult> OnPostRejectPayment()
        {
            RejectPayment.AdminId = _authHelper.CurrentAccountId();
            var op = await _orderApplication.RejectPaymentManuallyAsync(RejectPayment);

            TempData["Toast.Success"] = op.IsSucceeded;
            TempData["Toast.Message"] = op.Message;
            return RedirectToPage(new { id = RejectPayment.OrderId });
        }

        public async Task<IActionResult> OnPostConfirmPayment()
        {
            var op = await _orderApplication.ConfirmPaymentManuallyAsync(ConfirmPayment);

            TempData["Toast.Success"] = op.IsSucceeded;
            TempData["Toast.Message"] = op.Message;
            return RedirectToPage(new { id = ConfirmPayment.OrderId });
        }

        public async Task<IActionResult> OnPostProccessing(long orderId)
        {
            var op = await _orderApplication.ChangeStatusToProcessing(orderId);

            TempData["Toast.Success"] = op.IsSucceeded;
            TempData["Toast.Message"] = op.Message;
            return RedirectToPage(new { id = orderId });
        }

        public async Task<IActionResult> OnPostShip()
        {
            var op = await _orderApplication.ChangeStatusToShipped(Ship);

            if (!op.IsSucceeded)
            {
                TempData["Toast.Success"] = false;
                TempData["Toast.Message"] = op.Message;
                return RedirectToPage(new { id = Ship.OrderId });
            }

            BackgroundJob.Enqueue<PostTrackingSmsJob>(
                job => job.SendAsync(Ship.Mobile, Ship.PostTrackingNo));

            TempData["Toast.Success"] = true;
            TempData["Toast.Message"] = op.Message;
            return RedirectToPage(new { id = Ship.OrderId });
        }

        public async Task<IActionResult> OnPostResendTrackingSms(long orderId)
        {
            var adminId = _authHelper.CurrentAccountId();

            var trackingSmsDto = await _orderApplication.GetOrderForTrackingSms(orderId);
            if (trackingSmsDto == null)
            {
                TempData["Toast.Success"] = false;
                TempData["Toast.Message"] = "سفارش یافت نشد";
                return RedirectToPage(new { id = orderId });
            }

            if (!_adminSmsRateLimiter.CanResend(orderId, adminId))
            {
                TempData["Toast.Success"] = false;
                TempData["Toast.Message"] = "ارسال مجدد بیش از حد مجاز است. کمی بعد تلاش کنید.";
                return RedirectToPage(new { id = orderId });
            }

            if (string.IsNullOrWhiteSpace(trackingSmsDto.PostTrackingNo)
                || string.IsNullOrWhiteSpace(trackingSmsDto.Mobile))
            {
                TempData["Toast.Success"] = false;
                TempData["Toast.Message"] = "اطلاعات سفارش یا کد رهگیری معتبر نیست";
                return RedirectToPage(new { id = orderId });
            }

            BackgroundJob.Enqueue<PostTrackingSmsJob>(
                job => job.SendAsync(trackingSmsDto.Mobile, trackingSmsDto.PostTrackingNo));

            TempData["Toast.Success"] = true;
            TempData["Toast.Message"] = "ارسال مجدد پیامک کد رهگیری در صف انجام شد";

            return RedirectToPage(new { id = orderId });
        }

        public async Task<IActionResult> OnPostDelivered(long orderId)
        {
            var op = await _orderApplication.ChangeStatusToDelivered(orderId);

            TempData["Toast.Success"] = op.IsSucceeded;
            TempData["Toast.Message"] = op.Message;
            return RedirectToPage(new { id = orderId });
        }
    }
}