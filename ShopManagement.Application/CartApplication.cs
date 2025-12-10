using _0_Framework.Application;
using InventoryManagement.Domain.InventoryAgg;
using ShopManagement.Application.Contracts.Cart;
using ShopManagement.Domain.CartAgg;

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

        public async Task<OperationResult> AddOrIncreaseCartItem(AddToCart command)
        {
            var op = new OperationResult();

            if (command.CartKey == null)
                return op.Failed("کلید سبد خرید نمی‌تواند خالی باشد!");

            var cart = await _cartRepository.GetByCartKeyAsync(command.CartKey);

            if (cart == null)
                return op.Failed("سبد خرید یافت نشد!");

            if (command.ProductId <= 0)
                return op.Failed("شناسه محصول نامعتبر است.");

            if (command.Quantity <= 0)
                return op.Failed("تعداد نمی‌تواند صفر یا منفی باشد!");

            var inv = await _inventoryRepository.GetBy(command.ProductId);
            if (inv == null)
                return op.Failed("محصول در انبار یافت نشد.");
            if (command.Quantity > inv.CalculateCurrentCount())
                return op.Failed($"تعداد درخواستی شما ({command.Quantity}) بیشتر از تعداد موجودی در انبار است.");

            cart.AddItem(command.ProductId, command.Quantity);

            await _cartRepository.SaveChangesAsync();

            return op.Succeeded("محصول با موفقیت به سبد خرید شما اضافه شد.");
        }

        public async Task<OperationResult> ClearCartItemsAsync(string cartKey)
        {
            var op = new OperationResult();

            if (cartKey == null)
                return op.Failed("کلید سبد خرید نمی‌تواند خالی باشد!");

            var cart = await _cartRepository.GetByCartKeyAsync(cartKey);

            if (cart == null)
                return op.Failed("سبد خرید یافت نشد!");

            foreach (var item in cart.Items)
                _cartRepository.DeleteCartItem(item);

            cart.Clear();
            await _cartRepository.SaveChangesAsync();
            return op.Succeeded("سبد خرید شما با موفقیت خالی شد");
        }

        public async Task<OperationResult> CreateCartAsync(string userId)
        {
            var op = new OperationResult();

            if (string.IsNullOrWhiteSpace(userId))
                return op.Failed("شناسه کاربر نامعتبر است.");

            if (!long.TryParse(userId, out var accountId))
                return op.Failed("شناسه کاربر معتبر نیست.");

            bool exists = await _cartRepository.ExistsAsync(x => x.AccountId == accountId);
            if (exists)
                return op.Failed("برای این کاربر قبلاً سبد خرید ایجاد شده است.");

            var cartKey = $"user:{accountId}";

            var cart = new Cart(cartKey, accountId);

            _cartRepository.Create(cart);
            await _cartRepository.SaveChangesAsync();

            return op.Succeeded("سبد خرید با موفقیت ایجاد شد.");
        }

        public async Task<OperationResult> RemoveFromCartAsync(RemoveFromCart command)
        {
            var op = new OperationResult();

            if (command.CartKey == null)
                return op.Failed("کلید سبد خرید نمی‌تواند خالی باشد!");

            var cart = await _cartRepository.GetByCartKeyAsync(command.CartKey);

            if (cart == null)
                return op.Failed("سبد خرید یافت نشد!");

            if (command.ProductId <= 0)
                return op.Failed($"محصول با آیدی {command.ProductId} یافت نشد!");

            var cartItem = cart.Items.SingleOrDefault(x => x.ProductId == command.ProductId); // استفاده از SingleOrDefault
            if (cartItem == null)
                return op.Failed("این محصول در سبد خرید شما وجود ندارد.");

            cart.RemoveItem(cartItem.ProductId);

            _cartRepository.DeleteCartItem(cartItem);
            await _cartRepository.SaveChangesAsync();

            return op.Succeeded("محصول با موفقیت از سبد خرید شما حذف شد.");
        }

        public Task<OperationResult> SyncAnonymousCartToUserAsync(string anonCartKey, long userId)
        {
            throw new NotImplementedException();
        }

        public async Task<OperationResult> UpdateCartItemAsync(UpdateCartItem command)
        {
            var op = new OperationResult();

            if (command.CartKey == null)
                return op.Failed("کلید سبد خرید نمی‌تواند خالی باشد!");

            var cart = await _cartRepository.GetByCartKeyAsync(command.CartKey);

            if (cart == null)
                return op.Failed("سبد خرید یافت نشد!");

            if (command.ProductId <= 0)
                return op.Failed($"محصول با آیدی {command.ProductId} یافت نشد!");

            if (command.Quantity <= 0)
                return op.Failed("تعداد نمی‌تواند صفر یا منفی باشد!");

            var inv = await _inventoryRepository.GetBy(command.ProductId);
            if (command.Quantity > inv.CalculateCurrentCount())
                return op.Failed($"تعداد درخواستی شما ({command.Quantity}) بیشتر از تعداد موجودی در انبار است.");

            var cartItem = cart.Items.FirstOrDefault(x => x.ProductId == command.ProductId);
            if (cartItem == null)
                return op.Failed("این محصول در سبد خرید شما وجود ندارد.");

            cart.UpdateItem(cartItem.ProductId, command.Quantity);

            await _cartRepository.UpdateAsync(cart);
            await _cartRepository.SaveChangesAsync();

            return op.Succeeded("تعداد محصول در سبد خرید شما با موفقیت به‌روزرسانی شد.");
        }
    }
}
