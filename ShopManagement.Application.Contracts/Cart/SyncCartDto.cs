using _0_Framework.Application;
using System.ComponentModel.DataAnnotations;

namespace ShopManagement.Application.Contracts.Cart
{
    /// <summary>
    /// Data transfer object for syncing the user's cart.
    /// </summary>
    public class SyncCartDto
    {
        /// <summary>
        /// A required list of items representing the desired state of the cart.
        /// </summary>
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        public List<SyncCartItem>? Items { get; set; }
    }

}