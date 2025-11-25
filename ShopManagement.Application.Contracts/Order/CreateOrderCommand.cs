namespace ShopManagement.Application.Contracts.Order
{
    public class CreateOrderCommand
    {
        public long CustomerId { get; set; }                 // must be provided (user must be logged in for real checkout)
        public string FullName { get; set; } = "";
        public string Mobile { get; set; } = "";
        public string Address { get; set; } = "";
        public string PaymentMethod { get; set; } = "";      // e.g. "Online", "COD", "BankTransfer"
        public List<CreateOrderItem> Items { get; set; } = new();
    }

}
