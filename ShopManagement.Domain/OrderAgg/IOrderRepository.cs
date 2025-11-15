using ShopManagement.Application.Contracts.Order;

namespace ShopManagement.Domain.OrderAgg
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(long orderId);
        Task CreateAsync(Order order);
        Task SaveChangesAsync();
        List<OrderItemViewModel> GetItems(long orderId);
        List<OrderViewModel> Search(OrderSearchModel searchModel);
    }
}