namespace _01_PharmaPlusQuery.Contracts.Order
{
    public interface IOrderQuery
    {
        Task<List<OrderSummaryQueryModel>> GetUserOrdersSummaryAsync(long userId, CancellationToken cancellation = default);
        Task<OrderQueryModel> GetOrderDetailsAsync(long orderId, long userId, CancellationToken cancellation = default);
    }
}
