using _0_Framework.Application;
using System.ComponentModel.DataAnnotations;

namespace ShopManagement.Application.Contracts.Cart
{
    /// <summary>
    /// Represents a single item in the cart synchronization request.
    /// </summary>
    public class SyncCartItem
    {
        /// <summary>
        /// The unique identifier of the product.
        /// </summary>
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [Range(1, long.MaxValue, ErrorMessage = ValidationMessages.IsRequired)]
        public long ProductId { get; set; }

        /// <summary>
        /// The desired quantity for the product.
        /// </summary>
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [Range(1, 200, ErrorMessage = ValidationMessages.InvalidInput)]
        public int Qty { get; set; }
    }

}