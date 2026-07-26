namespace ShopManagement.Application.Contracts.Order
{
    public class RejectReturnCommand
    {
        public long OrderId { get; set; }
        public long AdminId { get; set; }
        public string RejectionReason { get; set; }
    }
}
