namespace SiteManagement.Application.Contracts.Ticket
{
    public class TicketViewModel
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Username { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string? AdminReply { get; set; }
        public string CreationDate { get; set; }
        public string? AdminReplyDate { get; set; }
        public bool IsAnswered { get; set; }
        public string TrackingCode { get; set; }
    }
}
