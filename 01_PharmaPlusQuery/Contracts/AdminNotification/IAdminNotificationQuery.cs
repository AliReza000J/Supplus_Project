namespace _01_PharmaPlusQuery.Contracts.AdminNotification
{
    public interface IAdminNotificationQuery
    {
        Task<AdminNotificationDto> GetAdminNotifications();
        Task<List<NotificationItemDto>> GetAllAdminNotifications();
    }
}
