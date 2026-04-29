using _0_Framework.Application;
using System.ComponentModel.DataAnnotations;

namespace ShopManagement.Application.Contracts.Cart
{
    /// <summary>
    /// Represents a command to update the quantity of a specific item in the cart.
    /// </summary>
    public class UpdateCartItem
    {
        /// <summary>
        /// The unique identifier of the cart.
        /// </summary>
        public string? CartKey { get; set; }

        /// <summary>
        /// The unique identifier of the product.
        /// </summary>
        public long ProductId { get; set; }

        /// <summary>
        /// The new desired quantity for the product.
        /// </summary>
        public int Qty { get; set; }
    }

    /// <summary>
    /// Data transfer object specifying the new quantity.
    /// </summary>
    public class QtyDto
    {
        /// <summary>
        /// The desired quantity. Must be between 1 and 200.
        /// </summary>
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [Range(1, 200, ErrorMessage = ValidationMessages.InvalidInput)]
        public int Qty { get; set; }
    }
}
