using _0_Framework.Application;
using System.ComponentModel.DataAnnotations;

namespace ShopManagement.Application.Contracts.Cart
{
    /// <summary>
    /// Data transfer object for adding an item to the shopping cart.
    /// </summary>
    public class AddToCartDto
    {
        /// <summary>
        /// The unique identifier of the product to add to the cart.
        /// </summary>
        /// <example>101</example>
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [Range(1, long.MaxValue, ErrorMessage = ValidationMessages.IsRequired)]
        public long ProductId { get; set; }

        /// <summary>
        /// The desired quantity of the product to add. Maximum quantity is 200.
        /// </summary>
        /// <example>3</example>
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [Range(1, 200, ErrorMessage = ValidationMessages.InvalidInput)]
        public int Qty { get; set; }
    }
}
