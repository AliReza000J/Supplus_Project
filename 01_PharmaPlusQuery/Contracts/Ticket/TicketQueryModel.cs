namespace _01_PharmaPlusQuery.Contracts.Ticket
{
    public class TicketQueryModel
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string? AdminReply { get; set; }
        public string CreationDate { get; set; }
        public string? AdminReplyDate { get; set; }
        public bool IsAnswered { get; set; }
        public string TrackingCode { get; set; }
    }
}
