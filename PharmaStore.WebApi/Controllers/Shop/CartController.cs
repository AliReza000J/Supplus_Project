using _0_Framework.Application;
using _01_PharmaPlusQuery.Contracts.Cart;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopManagement.Application.Contracts.Cart;
using System.Security.Claims;

namespace PharmaStore.WebApi.Controllers.Shop
{
    /// <summary>
    /// Manages shopping cart operations for authenticated users.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ICartApplication _cartApplication;
        private readonly ICartQuery _cartQuery;

        /// <summary>
        /// Initializes a new instance of the CartController.
        /// </summary>
        public CartController(ICartApplication cartApplication, ICartQuery cartQuery)
        {
            _cartApplication = cartApplication;
            _cartQuery = cartQuery;
        }

        /// <summary>
        /// Retrieves the current user's shopping cart.
        /// </summary>
        /// <remarks>
        /// Fetches the active shopping cart and its items for the authenticated user based on their token.
        /// </remarks>
        /// <param name="cancellation">A token to cancel the operation.</param>
        /// <response code="200">Success. Returns the user's shopping cart details.</response>
        /// <response code="401">Unauthorized. The user is not authenticated.</response>
        /// <response code="404">Not Found. The shopping cart does not exist for this user.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<CartViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<CartViewModel>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<CartViewModel>>> GetCart(CancellationToken cancellation)
        {
            var cartKey = GetCartKey(User);
            var cart = await _cartQuery.GetCartAsync(cartKey, cancellation);
            if (cart == null)
                return NotFound(ApiResponse<CartViewModel>.Fail("سبد خرید یافت نشد."));
            return Ok(ApiResponse<CartViewModel>.Ok(cart));
        }

        /// <summary>
        /// Adds a new item to the shopping cart or increases the quantity if the item already exists.
        /// </summary>
        /// <remarks>
        /// This endpoint allows an authenticated user to add a product to their cart. If the product is already in the cart,
        /// its quantity will be increased based on the provided `Qty`.
        /// </remarks>
        /// <param name="input">The DTO containing the product ID and desired quantity.</param>
        /// <param name="cancellation">A token to cancel the operation.</param>
        /// <response code="200">Success. The item was added or its quantity increased in the cart.</response>
        /// <response code="400">Bad Request. The operation failed (e.g., invalid product ID, quantity out of range, or item not available).</response>
        /// <response code="401">Unauthorized. The user is not authenticated.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("items")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

        /// <summary>
        /// Increases the quantity of a specific item in the cart by one.
        /// </summary>
        /// <remarks>
        /// This endpoint increments the quantity of an existing product in the authenticated user's cart.
        /// </remarks>
        /// <param name="productId">The unique identifier of the product to increase.</param>
        /// <param name="cancellation">A token to cancel the operation.</param>
        /// <response code="200">Success. The item quantity was increased successfully.</response>
        /// <response code="400">Bad Request. The operation failed (e.g., product not found or insufficient stock).</response>
        /// <response code="401">Unauthorized. The user is not authenticated.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("items/increase/{productId:long}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> IncreaseItem(long productId, CancellationToken cancellation)
        {
            var cartKey = GetCartKey(User);
            var command = new IncOrDecItem { CartKey = cartKey, ProductId = productId };

            var result = await _cartApplication.IncreaseCartItem(command, cancellation);

            if (!result.IsSucceeded)
                return BadRequest(ApiResponse<object>.Fail(result.Message));

            return Ok(ApiResponse<object>.Ok(data: new object(), message: result.Message));
        }

        /// <summary>
        /// Decreases the quantity of a specific item in the cart by one.
        /// </summary>
        /// <remarks>
        /// This endpoint decrements the quantity of an existing product in the authenticated user's cart.
        /// If the quantity reaches zero, the item might be removed from the cart depending on business logic.
        /// </remarks>
        /// <param name="productId">The unique identifier of the product to decrease.</param>
        /// <param name="cancellation">A token to cancel the operation.</param>
        /// <response code="200">Success. The item quantity was decreased successfully.</response>
        /// <response code="400">Bad Request. The operation failed.</response>
        /// <response code="401">Unauthorized. The user is not authenticated.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("items/decrease/{productId:long}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DecreaseItem(long productId, CancellationToken cancellation)
        {
            var cartKey = GetCartKey(User);
            var command = new IncOrDecItem { CartKey = cartKey, ProductId = productId };

            var result = await _cartApplication.DecreaseCartItem(command, cancellation);

            if (!result.IsSucceeded)
                return BadRequest(ApiResponse<object>.Fail(result.Message));

            return Ok(ApiResponse<object>.Ok(data: new object(), message: result.Message));
        }

        /// <summary>
        /// Updates the quantity of a specific item in the user's shopping cart.
        /// </summary>
        /// <remarks>
        /// Replaces the current quantity of the specified product with the new quantity provided. 
        /// The user must be authenticated.
        /// </remarks>
        /// <param name="productId">The unique identifier of the product to update.</param>
        /// <param name="qty">The DTO containing the new desired quantity.</param>
        /// <param name="cancellation">A token to cancel the operation.</param>
        /// <response code="200">Success. The item quantity was updated successfully.</response>
        /// <response code="400">Bad Request. The operation failed (e.g., product not found in cart, validation error).</response>
        /// <response code="401">Unauthorized. The user is not authenticated.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("items/{productId}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

        /// <summary>
        /// Removes a specific item completely from the cart.
        /// </summary>
        /// <remarks>
        /// Deletes the specified product from the authenticated user's shopping cart, regardless of its quantity.
        /// </remarks>
        /// <param name="productId">The unique identifier of the product to remove.</param>
        /// <param name="cancellation">A token to cancel the operation.</param>
        /// <response code="200">Success. The item was removed from the cart.</response>
        /// <response code="400">Bad Request. The operation failed (e.g., item not found in cart).</response>
        /// <response code="401">Unauthorized. The user is not authenticated.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpDelete("items/{productId:long}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RemoveItem(long productId, CancellationToken cancellation)
        {
            var cartKey = GetCartKey(User);
            var command = new RemoveFromCart { CartKey = cartKey, ProductId = productId };

            var result = await _cartApplication.RemoveFromCartAsync(command, cancellation);

            if (!result.IsSucceeded)
                return BadRequest(ApiResponse<object>.Fail(result.Message));

            return Ok(ApiResponse<object>.Ok(data: new object(), message: result.Message));
        }

        /// <summary>
        /// Clears all items from the user's shopping cart.
        /// </summary>
        /// <remarks>
        /// Empties the entire shopping cart for the currently authenticated user.
        /// </remarks>
        /// <param name="cancellation">A token to cancel the operation.</param>
        /// <response code="200">Success. The cart was cleared successfully.</response>
        /// <response code="400">Bad Request. The operation failed to clear the cart.</response>
        /// <response code="401">Unauthorized. The user is not authenticated.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpDelete("items")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ClearCart(CancellationToken cancellation)
        {
            var cartKey = GetCartKey(User);
            var result = await _cartApplication.ClearCartItemsAsync(cartKey, cancellation);

            if (!result.IsSucceeded)
                return BadRequest(ApiResponse<object>.Fail(result.Message));

            return Ok(ApiResponse<object>.Ok(data: new object(), message: result.Message));
        }

        /// <summary>
        /// Synchronizes the user's shopping cart with a provided list of items.
        /// </summary>
        /// <remarks>
        /// This endpoint replaces the current state of the authenticated user's cart with the items provided in the request.
        /// Useful for merging guest carts after login or syncing offline states.
        /// </remarks>
        /// <param name="dto">The DTO containing the list of items to synchronize.</param>
        /// <param name="cancellation">A token to cancel the operation.</param>
        /// <response code="200">Success. The cart was synchronized successfully.</response>
        /// <response code="400">Bad Request. The operation failed (e.g., validation error, product not found).</response>
        /// <response code="401">Unauthorized. The user is not authenticated.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("sync")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SyncCart([FromBody] SyncCartDto dto, CancellationToken cancellation)
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

        /// <summary>
        /// Generates a unique cart key based on the authenticated user's ID.
        /// </summary>
        protected string GetCartKey(ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return $"user:{userIdClaim}";
        }
    }
}
