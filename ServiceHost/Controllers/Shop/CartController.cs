using _0_Framework.Application;
using _01_PharmaPlusQuery.Contracts.Cart;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopManagement.Application.Contracts.Cart;
using System.Security.Claims;

namespace ServiceHost.Controllers.Shop
{
    [Route("api/[controller]")]
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
        public async Task<ActionResult<ApiResponse<CartViewModel>>> GetCart(CancellationToken cancellation)
        {
            var cartKey = GetCartKey(User);
            var cart = await _cartQuery.GetCartAsync(cartKey, cancellation);
            if (cart == null)
                return NotFound(ApiResponse<CartViewModel>.Fail("سبد خرید یافت نشد."));
            return Ok(ApiResponse<CartViewModel>.Ok(cart));
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("items")]
        public async Task<IActionResult> AddOrIncreaseItem([FromBody] AddToCartDto input, CancellationToken cancellation)
        {
            var cartKey = GetCartKey(User);
            var command = new AddToCart
            {
                CartKey = cartKey,
                ProductId = input.ProductId,
                Qty = input.Qty
            };

            var result = await _cartApplication.AddOrIncreaseCartItem(command, cancellation);

            if (!result.IsSucceeded)
                return BadRequest(ApiResponse<object>.Fail(result.Message));

            return Ok(ApiResponse<object>.Ok(data: new object(), message: result.Message));
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("items/increase/{productId}")]
        public async Task<IActionResult> IncreaseItem(long productId, CancellationToken cancellation)
        {
            var cartKey = GetCartKey(User);
            var command = new IncOrDecItem { CartKey = cartKey, ProductId = productId };

            var result = await _cartApplication.IncreaseCartItem(command, cancellation);

            if (!result.IsSucceeded)
                return BadRequest(ApiResponse<object>.Fail(result.Message));

            return Ok(ApiResponse<object>.Ok(data: new object(), message: result.Message));
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("items/decrease/{productId}")]
        public async Task<IActionResult> DecreaseItem(long productId, CancellationToken cancellation)
        {
            var cartKey = GetCartKey(User);
            var command = new IncOrDecItem { CartKey = cartKey, ProductId = productId };

            var result = await _cartApplication.DecreaseCartItem(command, cancellation);

            if (!result.IsSucceeded)
                return BadRequest(ApiResponse<object>.Fail(result.Message));

            return Ok(ApiResponse<object>.Ok(data: new object(), message: result.Message));
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("items/{productId}")]
        public async Task<IActionResult> UpdateItem(long productId, [FromBody] QtyDto qty, CancellationToken cancellation)
        {
            var cartKey = GetCartKey(User);
            var command = new UpdateCartItem
            {
                Qty = qty.Qty,
                CartKey = cartKey,
                ProductId = productId
            };

            var result = await _cartApplication.UpdateCartItemAsync(command, cancellation);

            if (!result.IsSucceeded)
                return BadRequest(ApiResponse<object>.Fail(result.Message));

            return Ok(ApiResponse<object>.Ok(data: new object(), message: result.Message));
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpDelete("items/{productId}")]
        public async Task<IActionResult> RemoveItem(long productId, CancellationToken cancellation)
        {
            var cartKey = GetCartKey(User);
            var command = new RemoveFromCart { CartKey = cartKey, ProductId = productId };

            var result = await _cartApplication.RemoveFromCartAsync(command, cancellation);

            if (!result.IsSucceeded)
                return BadRequest(ApiResponse<object>.Fail(result.Message));

            return Ok(ApiResponse<object>.Ok(data: new object(), message: result.Message));
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpDelete("items")]
        public async Task<IActionResult> ClearCart(CancellationToken cancellation)
        {
            var cartKey = GetCartKey(User);
            var result = await _cartApplication.ClearCartItemsAsync(cartKey, cancellation);

            if (!result.IsSucceeded)
                return BadRequest(ApiResponse<object>.Fail(result.Message));

            return Ok(ApiResponse<object>.Ok(data: new object(), message: result.Message));
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("sync")]
        public async Task<IActionResult> SyncCart(SyncCartDto dto, CancellationToken cancellation)
        {
            var cartKey = GetCartKey(User);
            var command = new SyncCart
            {
                CartKey = cartKey,
                Items = dto.Items
            };

            var result = await _cartApplication.SyncCartAsync(command, cancellation);

            if (!result.IsSucceeded)
                return BadRequest(ApiResponse<object>.Fail(result.Message));

            return Ok(ApiResponse<object>.Ok(data: new object(), message: result.Message));
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
