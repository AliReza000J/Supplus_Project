using _0_Framework.Domain;
using ShopManagement.Application.Contracts.Order;

namespace ShopManagement.Domain.OrderAgg
{
    public interface IOrderRepository: IRepository<long, Order>
    {
        Task<bool> HasThisUserBoughtProduct(long userId, long productId);
        Task<Order?> GetWithItemsAsync(long orderId);
        Task<List<OrderItemViewModel>> GetItems(long orderId);
        Task<List<OrderViewModel>> Search(OrderSearchModel searchModel);
        Task<OrderViewModel?> GetDetails(long orderId);
        Task<bool> UserOwnsOrder(string userId, long orderId);
    }
}