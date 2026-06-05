namespace _01_PharmaPlusQuery.Contracts.Ticket
{
    public interface ITicketQuery
    {
        Task<TicketQueryModel?> GetTicketDetails(long id, long userId, CancellationToken cancellation);
        Task<List<TicketQueryModel>?> GetUserTickets(long userId, CancellationToken cancellation);
    }
}
