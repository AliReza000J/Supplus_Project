namespace SiteManagement.Application.Contracts.Ticket
{
    public class CreateTicket
    {
        public long UserId { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string? Email { get; set; }
    }

}
