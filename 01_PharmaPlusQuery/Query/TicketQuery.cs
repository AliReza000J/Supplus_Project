using _0_Framework.Application;
using _01_PharmaPlusQuery.Contracts.Ticket;
using Microsoft.EntityFrameworkCore;
using SiteManagement.Infrastructure.EFCore;

namespace _01_PharmaPlusQuery.Query
{
    public class TicketQuery : ITicketQuery
    {
        private readonly SiteContext _siteContext;

        public TicketQuery(SiteContext siteContext)
        {
            _siteContext = siteContext;
        }

        public async Task<TicketQueryModel?> GetTicketDetailsByCode(string code, string userId, CancellationToken cancellation)
        {
            if (!long.TryParse(userId, out var currentUserId))
                return null;

            return await _siteContext.Tickets
                .Where(t =>
                    (t.TrackingCode == code || t.TrackingCode == code)
                    &&
                    t.UserId == currentUserId
                )
                .Select(t => new TicketQueryModel
                {
                    AdminReply = t.AdminReply,
                    CreationDate = t.CreationDate.ToFarsiFull(),
                    IsAnswered = t.IsAnswered,
                    UserId = t.UserId,
                    Message = t.Message,
                    Subject = t.Subject,
                    TrackingCode = t.TrackingCode,
                    AdminReplyDate = t.AnsweredAt != null
                        ? t.AnsweredAt.ToFarsiFull()
                        : null
                })
                .FirstOrDefaultAsync(cancellation);
        }


        public async Task<List<TicketQueryModel>?> GetUserTickets(string userId, CancellationToken cancellation)
        {
            if (!long.TryParse(userId, out var currentUserId))
                return null;

            return await _siteContext.Tickets
                .Where(t => t.UserId == currentUserId)
                .OrderByDescending(t => t.Id)
                .Select(t => new TicketQueryModel
                {
                    TrackingCode = t.TrackingCode,
                    Subject = t.Subject,
                    //Message = t.Message,
                    IsAnswered = t.IsAnswered,
                    CreationDate = t.CreationDate.ToFarsiFull(),
                    AdminReplyDate = t.AnsweredAt != null
                        ? t.AnsweredAt.ToFarsiFull()
                        : null,
                    UserId = t.UserId,
                })
                .ToListAsync(cancellation);
        }
    }
}
