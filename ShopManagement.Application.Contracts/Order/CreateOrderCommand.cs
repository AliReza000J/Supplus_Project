namespace ShopManagement.Application.Contracts.Order
{
    /// <summary>
    /// The internal command model for creating a new order, extending the base DTO with the user's ID.
    /// </summary>
    public class CreateOrderCommand : CreateOrderCommandDto
    {
        /// <summary>
        /// The unique identifier of the authenticated user placing the order.
        /// </summary>
        public long UserId { get; set; }
    }
}
