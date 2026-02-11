namespace _01_PharmaPlusQuery.Contracts.Ticket
{
    public interface ITicketQuery
    {
        Task<TicketQueryModel?> GetByCode(string code, string userId, CancellationToken cancellation);
        Task<List<TicketQueryModel>?> GetUserTickets(string userId, CancellationToken cancellation);
    }
}
