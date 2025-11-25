using _0_Framework.Application;
using ShopManagement.Application.Contracts.Order;

namespace ShopManagement.Application
{

    public class OrderApplication : IOrderApplication
    {
        public Task<OperationResult> ChangeStatusAsync(ChangeOrderStatus command)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult> ConfirmPaymentManuallyAsync(ConfirmPaymentManually command)
        {
            throw new NotImplementedException();
        }

        public Task<OrderViewModel?> GetAsync(long id)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult> PlaceOrderAsync(CreateOrderCommand command)
        {
            throw new NotImplementedException();
        }

        public Task<List<OrderViewModel>> SearchAsync(OrderSearchModel searchModel)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult> SetTrackingNumberAsync(SetTrackingNumber command)
        {
            throw new NotImplementedException();
        }
    }
}
