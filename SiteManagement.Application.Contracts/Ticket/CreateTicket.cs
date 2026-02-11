using _0_Framework.Application;
using System.ComponentModel.DataAnnotations;

namespace SiteManagement.Application.Contracts.Ticket
{
    public class CreateTicket
    {
        public long UserId { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string? Email { get; set; }
    }

    public class CreateTicketDto
    {
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [MaxLength(200, ErrorMessage = ValidationMessages.MaxLenght)]
        public string Subject { get; set; }

        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [MinLength(20, ErrorMessage = ValidationMessages.MaxLenght)]
        public string Message { get; set; }

        [EmailAddress(ErrorMessage = ValidationMessages.InvalidInput)]
        [MaxLength(200, ErrorMessage = ValidationMessages.MaxLenght)]
        public string? Email { get; set; }
    }
}
