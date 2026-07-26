namespace ShopManagement.Application.Contracts.Order
{
    public class ApproveReturnCommand
    {
        public long OrderId { get; set; }
        public long AdminId { get; set; }
        public string Notes { get; set; }
    }
}
