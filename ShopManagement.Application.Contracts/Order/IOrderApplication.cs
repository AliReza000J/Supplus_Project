using _0_Framework.Application;

namespace ShopManagement.Application.Contracts.Order
{
    public interface IOrderApplication
    {
        Task<(PlaceOrderResult?, OperationResult)> PlaceOrderAsync(CreateOrderCommand command); // create order (user cart)
        Task<OperationResult> VerifyGatewayPaymentAsync(VerifyGatewayPaymentCommand command);
        Task<OperationResult> RejectPaymentManuallyAsync(RejectPaymentManually command);
        Task<OperationResult> ConfirmPaymentManuallyAsync(ConfirmPaymentManually command);
        Task<OperationResult> ChangeStatusToProcessing(long oederId);
        Task<OperationResult> ChangeStatusToShipped(ShipCommand command);
        Task<OperationResult> ChangeStatusToDelivered(long oederId);
        Task<OrderTrackingSmsDto> GetOrderForTrackingSms(long orderId);
        Task<List<OrderItemViewModel>> GetItems(long orderId);
        Task<List<OrderViewModel>> SearchAsync(OrderSearchModel searchModel);
        Task<OrderViewModel?> GetDetails(long orderId);

        List<OrderStatusDto> GetStatuses();
        Task<bool> UserOwnsOrder(string userId, long orderId);
    }
}
