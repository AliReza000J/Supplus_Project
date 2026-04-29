namespace ShopManagement.Application.Contracts.Cart
{
    /// <summary>
    /// Represents the command to add or increase the quantity of an item in the cart. Inherits from AddToCartDto.
    /// </summary>
    public class AddToCart : AddToCartDto
    {
        /// <summary>
        /// The unique key identifying the user's cart.
        /// </summary>
        public string? CartKey { get; set; }
    }
}
