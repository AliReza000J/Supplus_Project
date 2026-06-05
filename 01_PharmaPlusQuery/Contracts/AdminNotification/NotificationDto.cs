namespace _01_PharmaPlusQuery.Contracts.AdminNotification
{
    public class NotificationItemDto
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string CreationDate { get; set; }
        public DateTime ActualDate { get; set; }
        public string Link { get; set; }
        public string IconClass { get; set; }
        public string ColorClass { get; set; }
    }

    public class AdminNotificationDto
    {
        public int NewTicketsCount { get; set; }
        public int PendingOrdersCount { get; set; }
        public int RefundRequestsCount { get; set; }

        public int TotalUnreadCount => NewTicketsCount + PendingOrdersCount + RefundRequestsCount;

        public List<NotificationItemDto> RecentNotifications { get; set; }

        public AdminNotificationDto()
        {
            RecentNotifications = new List<NotificationItemDto>();
        }
    }
}
