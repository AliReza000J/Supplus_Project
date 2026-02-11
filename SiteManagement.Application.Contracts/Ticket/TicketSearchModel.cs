namespace SiteManagement.Application.Contracts.Ticket
{
    public class TicketSearchModel
    {
        public string? Username { get; set; }
        public bool ShowPendingOnly { get; set; } = false;
    }
}
