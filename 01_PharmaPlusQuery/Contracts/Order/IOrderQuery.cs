namespace _01_PharmaPlusQuery.Contracts.Order
{
    public interface IOrderQuery
    {
        Task<List<OrderQueryModel>> GetOrdersByUser(long userId);
    }
}
