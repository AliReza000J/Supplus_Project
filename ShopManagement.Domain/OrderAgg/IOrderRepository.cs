using _0_Framework.Domain;
using ShopManagement.Application.Contracts.Order;

namespace ShopManagement.Domain.OrderAgg
{
    public interface IOrderRepository: IRepository<long, Order>
    {
        Task<bool> HasThisUserBoughtProduct(long userId, long productId, CancellationToken cancellation);
        Task<Order?> GetWithItemsAsync(long orderId, CancellationToken cancellation);
        Task<List<OrderItemViewModel>> GetItems(long orderId, CancellationToken cancellation);
        Task<List<OrderViewModel>> Search(OrderSearchModel searchModel, CancellationToken cancellation);
        Task<OrderViewModel?> GetDetails(long orderId, CancellationToken cancellation);
        Task<bool> UserOwnsOrder(long userId, long orderId, CancellationToken cancellation);
    }
}