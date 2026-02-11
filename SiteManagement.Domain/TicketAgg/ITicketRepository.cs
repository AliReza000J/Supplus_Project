using _0_Framework.Domain;
using SiteManagement.Application.Contracts.Ticket;

namespace SiteManagement.Domain.TicketAgg
{
    public interface ITicketRepository : IRepository<long, Ticket>
    {
        Task<ReplyTicket?> GetTicketDetails(long ticketId, CancellationToken cancellation);
        Task<List<TicketViewModel>> Search(TicketSearchModel searchModel, CancellationToken cancellation);
    }
}
