using _0_Framework.Domain;
using _0_Framework.Application;

namespace SiteManagement.Domain.TicketAgg
{
    public class Ticket : EntityBase
    {
        public long UserId { get; private set; }
        public string? Email { get; private set; }

        public string Subject { get; private set; }
        public string Message { get; private set; }

        public long? AdminId { get; private set; }
        public string? AdminReply { get; private set; }
        public bool IsAnswered { get; private set; }

        public string TrackingCode { get; private set; }

        public DateTime? AnsweredAt { get; private set; }

        private Ticket() { }

        public Ticket(long userId, string? email, string subject, string message, string trackingCode)
        {
            UserId = userId;
            Email = email;
            Subject = subject;
            Message = message;
            TrackingCode = trackingCode;
            IsAnswered = false;
        }

        public void Reply(string reply, long adminId)
        {
            AdminReply = reply;
            IsAnswered = true;
            AnsweredAt = DateTime.Now;
            AdminId = adminId;
        }
    }
}
