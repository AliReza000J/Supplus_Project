using _0_Framework.Application;
using _0_Framework.Domain;
using _0_Framework.Infrastructure;
using AccountManagement.Infrastructure.EFCore;
using Microsoft.EntityFrameworkCore;
using SiteManagement.Application.Contracts.Ticket;
using SiteManagement.Domain.TicketAgg;

namespace SiteManagement.Infrastructure.EFCore.Repository
{
    public class TicketRepository : RepositoryBase<long, Ticket>, ITicketRepository
    {
        private readonly SiteContext _context;
        private readonly AccountContext _accountContext;
        public TicketRepository(SiteContext context, AccountContext accountContext) : base(context)
        {
            _context = context;
            _accountContext = accountContext;
        }

        public async Task<List<TicketViewModel>> Search(TicketSearchModel searchModel, CancellationToken cancellation)
        {
            var ticketsQuery = _context.Tickets
                .AsQueryable();

            if (searchModel.ShowPendingOnly)
            {
                ticketsQuery = ticketsQuery
                    .Where(t => t.IsAnswered == false);
            }

            var tickets = await ticketsQuery
                .OrderByDescending(t => t.Id)
                .Select(t => new TicketViewModel
                {
                    Id = t.Id,
                    AdminReply = t.AdminReply,
                    CreationDate = t.CreationDate.ToFarsi(),
                    IsAnswered = t.IsAnswered,
                    Message = t.Message,
                    Subject = t.Subject,
                    TrackingCode = t.TrackingCode,
                    AdminReplyDate = t.AnsweredAt != null
                        ? t.AnsweredAt.ToFarsi()
                        : null,
                    UserId = t.UserId
                })
                .AsNoTracking()
                .ToListAsync(cancellation);

            if (!tickets.Any())
                return tickets;

            var userIds = tickets
                .Select(t => t.UserId)
                .Distinct()
                .ToList();

            var users = await _accountContext.Accounts
                .Where(a => userIds.Contains(a.Id))
                .Select(a => new { a.Id, a.Username })
                .AsNoTracking()
                .ToListAsync(cancellation);

            var userDict = users.ToDictionary(x => x.Id, x => x.Username);

            foreach (var ticket in tickets)
            {
                userDict.TryGetValue(ticket.UserId, out var username);
                ticket.Username = username;
            }

            if (!string.IsNullOrWhiteSpace(searchModel.Username))
            {
                tickets = tickets
                    .Where(t => t.Username.Contains(searchModel.Username))
                    .ToList();
            }

            return tickets;
        }



        public async Task<ReplyTicket?> GetTicketDetails(long ticketId, CancellationToken cancellation)
        {
            var ticket = await _context.Tickets
                .AsNoTracking()
                .Where(t => t.Id == ticketId)
                .Select(t => new ReplyTicket
                {
                    TicketId = t.Id,
                    ReplyMessage = t.AdminReply ?? "",
                    IsAnswered = t.IsAnswered,
                    Message = t.Message,
                    Subject = t.Subject,
                })
                .FirstOrDefaultAsync(cancellation);

            if (ticket == null)
                return null;

            return ticket;

        }
    }
}
