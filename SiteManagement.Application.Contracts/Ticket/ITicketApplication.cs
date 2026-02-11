using _0_Framework.Application;

namespace SiteManagement.Application.Contracts.Ticket
{
    public interface ITicketApplication
    {
        Task<(string, OperationResult)> CreateTicket(CreateTicket command, CancellationToken cancellation);
        Task<OperationResult> ReplyTicket(ReplyTicket command, CancellationToken cancellation);
        Task<ReplyTicket?> GetDetailsAsync(long id, CancellationToken cancellation);
        Task<List<TicketViewModel>> Search(TicketSearchModel searchModel, CancellationToken cancellation);
    }
}
