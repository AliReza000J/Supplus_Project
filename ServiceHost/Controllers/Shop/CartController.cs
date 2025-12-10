using _0_Framework.Application;
using _01_PharmaPlusQuery.Contracts.Cart;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopManagement.Application.Contracts.Cart;
using System.Security.Claims;

namespace ServiceHost.Controllers.Shop
{
    [Route("shop/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ICartApplication _cartApplication;
        private readonly ICartQuery _cartQuery;

        public CartController(ICartApplication cartApplication, ICartQuery cartQuery)
        {
            _cartApplication = cartApplication;
            _cartQuery = cartQuery;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        public async Task<ActionResult<ApiResponse<CartViewModel>>> GetCart()
        {
            var cartKey = GetCartKey(User);
            var cart = await _cartQuery.GetCartAsync(cartKey);
            if (cart == null)
                return NotFound(ApiResponse<CartViewModel>.Fail("سبد خرید یافت نشد."));
            return Ok(ApiResponse<CartViewModel>.Ok(cart));
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("items")]
        public async Task<IActionResult> AddOrIncreaseItem(long ProductId, int qty)
        {
            var cartKey = GetCartKey(User);

            var command = new AddToCart
            {
                CartKey = cartKey,
                ProductId = ProductId,
                Quantity = qty
            };

            var result = await _cartApplication.AddOrIncreaseCartItem(command);

            if (!result.IsSucceeded)
                return BadRequest(ApiResponse<object>.Fail(result.Message));

            return Ok(ApiResponse<object>.Ok(result));
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("items/{productId}")]
        public async Task<IActionResult> UpdateItem(long productId, int qty)
        {
            var cartKey = GetCartKey(User);
            var command = new UpdateCartItem
            {
                Quantity = qty,
                CartKey = cartKey,
                ProductId = productId
            };

            var result = await _cartApplication.UpdateCartItemAsync(command);

            if (!result.IsSucceeded)
                return BadRequest(ApiResponse<object>.Fail(result.Message));

            return Ok(ApiResponse<object>.Ok(result));
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpDelete("items/{productId}")]
        public async Task<IActionResult> RemoveItem(long productId)
        {
            var cartKey = GetCartKey(User);
            var command = new RemoveFromCart { CartKey = cartKey, ProductId = productId };

            var result = await _cartApplication.RemoveFromCartAsync(command);

            if (!result.IsSucceeded)
                return BadRequest(ApiResponse<string>.Fail(result.Message));

            return Ok(ApiResponse<string>.Ok(result.Message));
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpDelete("items")]
        public async Task<IActionResult> ClearCart()
        {
            var cartKey = GetCartKey(User);
            var result = await _cartApplication.ClearCartItemsAsync(cartKey);

            if (!result.IsSucceeded)
                return BadRequest(ApiResponse<string>.Fail(result.Message));

            return Ok(ApiResponse<string>.Ok(result.Message));
        }

        protected string GetCartKey(ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return $"user:{userIdClaim}";
        }

        //protected string GetCartKey(ClaimsPrincipal user, string? guestKey = null)
        //{
        //    var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        //    if (!string.IsNullOrEmpty(userIdClaim))
        //        return $"user:{userIdClaim}";

        //    if (!string.IsNullOrEmpty(guestKey))
        //        return $"anon:{guestKey}";

        //    return $"anon:{Guid.NewGuid()}";
        //}

        // [FromHeader(Name = "X-Cart-Key")] string? guestKey

    }
}
