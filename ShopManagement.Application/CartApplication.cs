using _0_Framework.Application;
using InventoryManagement.Domain.InventoryAgg;
using ShopManagement.Application.Contracts.Cart;
using ShopManagement.Domain.CartAgg;
using System.Text.RegularExpressions;

namespace ShopManagement.Application
{
    public class CartApplication : ICartApplication
    {
        private readonly ICartRepository _cartRepository;
        private readonly IInventoryRepository _inventoryRepository;
        public CartApplication(ICartRepository cartRepository, IInventoryRepository inventoryRepository)
        {
            _cartRepository = cartRepository;
            _inventoryRepository = inventoryRepository;
        }

        public async Task<OperationResult> AddOrIncreaseCartItem(AddToCart command, CancellationToken cancellation)
        {
            var op = new OperationResult();

            if (command.CartKey == null)
                return op.Failed("کلید سبد خرید نمی‌تواند خالی باشد!");

            if (!Regex.IsMatch(command.CartKey, @"^user:\d+$"))
                return op.Failed("مقدار کلید سبد خرید نامعتبر است!.");

            var cart = await _cartRepository.GetOrCreateByCartKeyAsync(command.CartKey, cancellation);

            if (cart == null)
                return op.Failed("سبد خرید یافت نشد!");

            if (command.ProductId <= 0)
                return op.Failed("شناسه محصول نامعتبر است.");

            if (command.Qty <= 0)
                return op.Failed("تعداد نمی‌تواند صفر یا منفی باشد!");

            var inv = await _inventoryRepository.GetBy(command.ProductId, cancellation);
            if (inv == null)
                return op.Failed("محصول در انبار یافت نشد.");

            var currentStock = inv.CalculateCurrentCount();
            var existingItem = cart.Items.FirstOrDefault(x => x.ProductId == command.ProductId);
            var requestedTotal = command.Qty + (existingItem?.Quantity ?? 0);

            if (requestedTotal > currentStock)
                return op.Failed($"تعداد درخواستی شما ({requestedTotal}) بیشتر از تعداد موجودی در انبار است.");

            cart.AddItem(command.ProductId, command.Qty);

            await _cartRepository.SaveChangesAsync(cancellation);

            return op.Succeeded("محصول با موفقیت به سبد خرید شما اضافه شد.");
        }

        public async Task<OperationResult> ClearCartItemsAsync(string cartKey, CancellationToken cancellation)
        {
            var op = new OperationResult();

            if (cartKey == null)
                return op.Failed("کلید سبد خرید نمی‌تواند خالی باشد!");

            if (!Regex.IsMatch(cartKey, @"^user:\d+$"))
                return op.Failed("مقدار کلید سبد خرید نامعتبر است!.");

            var cart = await _cartRepository.GetOrCreateByCartKeyAsync(cartKey, cancellation);

            if (cart == null)
                return op.Failed("سبد خرید یافت نشد!");

            foreach (var item in cart.Items)
                _cartRepository.DeleteCartItem(item);

            cart.Clear();
            await _cartRepository.SaveChangesAsync(cancellation);
            return op.Succeeded("سبد خرید شما با موفقیت خالی شد");
        }

        public async Task<OperationResult> CreateCartAsync(long userId, CancellationToken cancellation)
        {
            var op = new OperationResult();

            if (userId <= 0)
                return op.Failed("شناسه کاربر نامعتبر است.");

            bool exists = await _cartRepository.ExistsAsync(x => x.UserId == userId, cancellation);
            if (exists)
                return op.Failed("برای این کاربر قبلاً سبد خرید ایجاد شده است.");

            var cartKey = $"user:{userId}";

            var cart = new Cart(cartKey, userId);

            _cartRepository.Create(cart);
            await _cartRepository.SaveChangesAsync(cancellation);

            return op.Succeeded("سبد خرید با موفقیت ایجاد شد.");
        }

        public async Task<OperationResult> DecreaseCartItem(IncOrDecItem command, CancellationToken cancellation)
        {
            var op = new OperationResult();

            if (command.CartKey == null)
                return op.Failed("کلید سبد خرید نمی‌تواند خالی باشد!");

            if (!Regex.IsMatch(command.CartKey, @"^user:\d+$"))
                return op.Failed("مقدار کلید سبد خرید نامعتبر است!.");

            var cart = await _cartRepository.GetOrCreateByCartKeyAsync(command.CartKey, cancellation);

            if (cart == null)
                return op.Failed("سبد خرید یافت نشد!");

            if (command.ProductId <= 0)
                return op.Failed($"محصول با آیدی {command.ProductId} یافت نشد!");

            var cartItem = cart.Items.SingleOrDefault(x => x.ProductId == command.ProductId); // استفاده از SingleOrDefault
            if (cartItem == null)
                return op.Failed("این محصول در سبد خرید شما وجود ندارد.");

            if (cartItem.Quantity == 1)
            {
                cart.RemoveItem(cartItem.ProductId);
                _cartRepository.DeleteCartItem(cartItem);
            }
            else // if (cartItem.Quantity > 1)
            {
                cart.DecreaseItem(cartItem.ProductId);
                await _cartRepository.UpdateAsync(cart, cancellation);
            }

            await _cartRepository.SaveChangesAsync(cancellation);

            return op.Succeeded();
        }

        public async Task<OperationResult> IncreaseCartItem(IncOrDecItem command, CancellationToken cancellation)
        {
            var op = new OperationResult();

            if (command.CartKey == null)
                return op.Failed("کلید سبد خرید نمی‌تواند خالی باشد!");

            if (!Regex.IsMatch(command.CartKey, @"^user:\d+$"))
                return op.Failed("مقدار کلید سبد خرید نامعتبر است!.");

            var cart = await _cartRepository.GetOrCreateByCartKeyAsync(command.CartKey, cancellation);

            if (cart == null)
                return op.Failed("سبد خرید یافت نشد!");

            if (command.ProductId <= 0)
                return op.Failed($"محصول با آیدی {command.ProductId} یافت نشد!");

            var cartItem = cart.Items.SingleOrDefault(x => x.ProductId == command.ProductId); // استفاده از SingleOrDefault
            if (cartItem == null)
                return op.Failed("این محصول در سبد خرید شما وجود ندارد.");

            var inv = await _inventoryRepository.GetBy(command.ProductId, cancellation);
            if (inv == null)
                return op.Failed("محصول در انبار یافت نشد.");

            var currentStock = inv.CalculateCurrentCount();

            if (cartItem.Quantity + 1 > currentStock)
                return op.Failed($"تعداد درخواستی شما ({cartItem.Quantity + 1}) بیشتر از تعداد موجودی در انبار است.");

            cart.IncreaseItem(cartItem.ProductId);

            await _cartRepository.UpdateAsync(cart, cancellation);
            await _cartRepository.SaveChangesAsync(cancellation);

            return op.Succeeded();
        }

        public async Task<OperationResult> RemoveFromCartAsync(RemoveFromCart command, CancellationToken cancellation)
        {
            var op = new OperationResult();

            if (command.CartKey == null)
                return op.Failed("کلید سبد خرید نمی‌تواند خالی باشد!");

            if (!Regex.IsMatch(command.CartKey, @"^user:\d+$"))
                return op.Failed("مقدار کلید سبد خرید نامعتبر است!.");

            var cart = await _cartRepository.GetOrCreateByCartKeyAsync(command.CartKey, cancellation);

            if (cart == null)
                return op.Failed("سبد خرید یافت نشد!");

            if (command.ProductId <= 0)
                return op.Failed($"محصول با آیدی {command.ProductId} یافت نشد!");

            var cartItem = cart.Items.SingleOrDefault(x => x.ProductId == command.ProductId); // استفاده از SingleOrDefault
            if (cartItem == null)
                return op.Failed("این محصول در سبد خرید شما وجود ندارد.");

            cart.RemoveItem(cartItem.ProductId);

            _cartRepository.DeleteCartItem(cartItem);
            await _cartRepository.SaveChangesAsync(cancellation);

            return op.Succeeded("محصول با موفقیت از سبد خرید شما حذف شد.");
        }

        public async Task<OperationResult> SyncCartAsync(SyncCart command, CancellationToken cancellation)
        {
            var op = new OperationResult();

            if (command.CartKey == null)
                return op.Failed("کلید سبد خرید نمی‌تواند خالی باشد!");

            if (!Regex.IsMatch(command.CartKey, @"^user:\d+$"))
                return op.Failed("مقدار کلید سبد خرید نامعتبر است!.");

            var cart = await _cartRepository.GetOrCreateByCartKeyAsync(command.CartKey, cancellation);

            if (cart == null)
                return op.Failed("سبد خرید یافت نشد!");

            if (!cart.Items.Any() && command.Items != null && command.Items.Any())
            {
                foreach(var item in command.Items)
                {
                    if (item.ProductId <= 0)
                        //return op.Failed("شناسه محصول نامعتبر است.");
                        continue;

                    if (item.Qty <= 0)
                        //return op.Failed("تعداد نمی‌تواند صفر یا منفی باشد!");
                        continue;

                    var inv = await _inventoryRepository.GetBy(item.ProductId, cancellation);
                    if (inv == null)
                        //return op.Failed("محصول در انبار یافت نشد.");
                        continue;

                    var currentStock = inv.CalculateCurrentCount();
                    var existingItem = cart.Items.FirstOrDefault(x => x.ProductId == item.ProductId);
                    var requestedTotal = item.Qty + (existingItem?.Quantity ?? 0);

                    if (requestedTotal > currentStock)
                        item.Qty = (int)currentStock;

                    cart.AddItem(item.ProductId, item.Qty);
                }

                await _cartRepository.SaveChangesAsync(cancellation);
                return op.Succeeded("سبد خرید شما با موفقیت به‌هنگام شد.");
            }
            // else: syncing is not available, let the cart be unchanged.
            return op.Warn("سبد خرید به‌هنگام نشد.");
        }

        public async Task<OperationResult> UpdateCartItemAsync(UpdateCartItem command, CancellationToken cancellation)
        {
            var op = new OperationResult();

            if (command.CartKey == null)
                return op.Failed("کلید سبد خرید نمی‌تواند خالی باشد!");

            if (!Regex.IsMatch(command.CartKey, @"^user:\d+$"))
                return op.Failed("مقدار کلید سبد خرید نامعتبر است!.");

            var cart = await _cartRepository.GetOrCreateByCartKeyAsync(command.CartKey, cancellation);

            if (cart == null)
                return op.Failed("سبد خرید یافت نشد!");

            if (command.ProductId <= 0)
                return op.Failed($"محصول با آیدی {command.ProductId} یافت نشد!");

            if (command.Qty <= 0)
                return op.Failed("تعداد نمی‌تواند صفر یا منفی باشد!");

            var cartItem = cart.Items.FirstOrDefault(x => x.ProductId == command.ProductId);
            if (cartItem == null)
                return op.Failed("این محصول در سبد خرید شما وجود ندارد.");

            var inv = await _inventoryRepository.GetBy(command.ProductId, cancellation);
            if (inv == null)
                return op.Failed("محصول در انبار یافت نشد.");

            var currentStock = inv.CalculateCurrentCount();
            var existingItem = cart.Items.FirstOrDefault(x => x.ProductId == command.ProductId);
            var requestedTotal = command.Qty + (existingItem?.Quantity ?? 0);

            if (requestedTotal > currentStock)
                return op.Failed($"تعداد درخواستی شما ({requestedTotal}) بیشتر از تعداد موجودی در انبار است.");

            cart.UpdateItem(cartItem.ProductId, command.Qty);

            await _cartRepository.UpdateAsync(cart, cancellation);
            await _cartRepository.SaveChangesAsync(cancellation);

            return op.Succeeded("تعداد محصول در سبد خرید شما با موفقیت به‌روزرسانی شد.");
        }
    }
}
