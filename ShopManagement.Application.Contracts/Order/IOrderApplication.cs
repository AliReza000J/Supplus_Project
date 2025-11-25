using _0_Framework.Application;

namespace ShopManagement.Application.Contracts.Order
{
    public interface IOrderApplication
    {
        Task<OperationResult> PlaceOrderAsync(CreateOrderCommand command); // create order (user or admin)
        Task<OperationResult> ChangeStatusAsync(ChangeOrderStatus command);
        Task<OperationResult> SetTrackingNumberAsync(SetTrackingNumber command);
        Task<OperationResult> ConfirmPaymentManuallyAsync(ConfirmPaymentManually command);

        Task<OrderViewModel?> GetAsync(long id);
        Task<List<OrderViewModel>> SearchAsync(OrderSearchModel searchModel);
    }
}
