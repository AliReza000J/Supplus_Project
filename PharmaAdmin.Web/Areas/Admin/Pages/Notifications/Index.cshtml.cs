using _01_PharmaPlusQuery.Contracts.AdminNotification;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PharmaAdmin.Web.Areas.Admin.Pages.Notifications
{
    public class IndexModel : PageModel
    {
        private readonly IAdminNotificationQuery _notificationQuery;

        public IndexModel(IAdminNotificationQuery notificationQuery)
        {
            _notificationQuery = notificationQuery;
        }

        public List<NotificationItemDto> Notifications { get; set; }

        public async Task OnGet()
        {
            Notifications = await _notificationQuery.GetAllAdminNotifications();
        }
    }
}
