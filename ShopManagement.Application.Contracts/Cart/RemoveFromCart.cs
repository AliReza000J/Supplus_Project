using _0_Framework.Application;
using System.ComponentModel.DataAnnotations;

namespace ShopManagement.Application.Contracts.Cart
{
    public class RemoveFromCart
    {
        public string? CartKey { get; set; }
        public long ProductId { get; set; }
    }
}
