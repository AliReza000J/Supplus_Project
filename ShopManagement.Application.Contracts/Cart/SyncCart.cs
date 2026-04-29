namespace ShopManagement.Application.Contracts.Cart
{
    /// <summary>
    /// Represents a command to synchronize the cart with a new set of items.
    /// </summary>
    public class SyncCart : SyncCartDto
    {
        /// <summary>
        /// The unique identifier of the cart.
        /// </summary>
        public string? CartKey { get; set; }
    }

}