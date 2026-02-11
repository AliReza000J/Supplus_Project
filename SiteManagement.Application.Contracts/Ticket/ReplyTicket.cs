using _0_Framework.Application;
using System.ComponentModel.DataAnnotations;

namespace SiteManagement.Application.Contracts.Ticket
{
    public class ReplyTicket
    {
        public long AdminId { get; set; }

        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [Range(1, long.MaxValue, ErrorMessage = ValidationMessages.IsRequired)]
        public long TicketId { get; set; }

        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        public string ReplyMessage { get; set; }

        public string? Subject { get; set; }
        public string? Message { get; set; }
        public bool IsAnswered { get; set; }
    }
}
