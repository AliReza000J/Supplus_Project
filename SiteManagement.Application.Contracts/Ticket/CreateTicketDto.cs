using _0_Framework.Application;
using System.ComponentModel.DataAnnotations;

namespace SiteManagement.Application.Contracts.Ticket
{
    /// <summary>
    /// Represents the data required to create a new support ticket.
    /// </summary>
    public class CreateTicketDto
    {
        /// <summary>
        /// The subject or title of the ticket.
        /// </summary>
        /// <example>Problem with my last order</example>
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [MaxLength(200, ErrorMessage = ValidationMessages.MaxLength)]
        public string Subject { get; set; }

        /// <summary>
        /// The main content or body of the ticket message. Must be at least 20 characters long.
        /// </summary>
        /// <example>Hello, I have an issue with order #12345. It hasn't arrived yet.</example>
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [MinLength(20, ErrorMessage = ValidationMessages.MinLength)]
        public string Message { get; set; }

        /// <summary>
        /// An optional email address for correspondence. If not provided, the user's primary email might be used.
        /// </summary>
        /// <example>user.contact@example.com</example>
        [EmailAddress(ErrorMessage = ValidationMessages.InvalidInput)]
        [MaxLength(200, ErrorMessage = ValidationMessages.MaxLength)]
        public string? Email { get; set; }
    }

}
