namespace ShopManagement.Application.Contracts.Order
{
    public class CreateOrderItem
    {
        public long ProductId { get; set; }
        public int Quantity { get; set; }
    }

}
