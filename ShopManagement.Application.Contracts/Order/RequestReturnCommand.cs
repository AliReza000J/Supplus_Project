namespace ShopManagement.Application.Contracts.Order
{
    public class RequestReturnDto
    {
        public long OrderId { get; set; }
        public string Reason { get; set; }
    }

    public class RequestReturnCommand: RequestReturnDto
    {
        public long UserId { get; set; }

    }
}
