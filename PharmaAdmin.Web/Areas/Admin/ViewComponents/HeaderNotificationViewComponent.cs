using _01_PharmaPlusQuery.Contracts.AdminNotification;
using Microsoft.AspNetCore.Mvc;

namespace PharmaAdmin.Web.Areas.Admin.ViewComponents
{
    public class HeaderNotificationViewComponent : ViewComponent
    {
        private readonly IAdminNotificationQuery _adminNotificationQuery;

        public HeaderNotificationViewComponent(IAdminNotificationQuery adminNotificationQuery)
        {
            _adminNotificationQuery = adminNotificationQuery;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var notifications = await _adminNotificationQuery.GetAdminNotifications();

            return View("HeaderNotification", notifications);
        }
    }
}
