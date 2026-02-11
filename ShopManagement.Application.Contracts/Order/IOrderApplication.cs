using _0_Framework.Application;

namespace ShopManagement.Application.Contracts.Order
{
    public interface IOrderApplication
    {
        Task<(PlaceOrderResult?, OperationResult)> PlaceOrderAsync(CreateOrderCommand command, CancellationToken cancellation); // create order (user cart)
        Task<OperationResult> VerifyGatewayPaymentAsync(VerifyGatewayPaymentCommand command, CancellationToken cancellation);
        Task<OperationResult> RejectPaymentManuallyAsync(RejectPaymentManually command, CancellationToken cancellation);
        Task<OperationResult> ConfirmPaymentManuallyAsync(ConfirmPaymentManually command, CancellationToken cancellation);
        Task<OperationResult> ChangeStatusToProcessing(long oederId, CancellationToken cancellation);
        Task<OperationResult> ChangeStatusToShipped(ShipCommand command, CancellationToken cancellation);
        Task<OperationResult> ChangeStatusToDelivered(long oederId, CancellationToken cancellation);
        Task<OrderTrackingSmsDto> GetOrderForTrackingSms(long orderId, CancellationToken cancellation);
        Task<List<OrderItemViewModel>> GetItems(long orderId, CancellationToken cancellation);
        Task<List<OrderViewModel>> SearchAsync(OrderSearchModel searchModel, CancellationToken cancellation);
        Task<OrderViewModel?> GetDetails(long orderId, CancellationToken cancellation);

        List<OrderStatusDto> GetStatuses();
        Task<bool> UserOwnsOrder(long userId, long orderId, CancellationToken cancellation);
    }
}
