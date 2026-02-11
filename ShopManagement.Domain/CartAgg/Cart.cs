using _0_Framework.Domain;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Text.RegularExpressions;

namespace ShopManagement.Domain.CartAgg
{
    public class Cart : BaseEntity
    {
        public long? UserId { get; private set; } = 0; // user id
        public string CartKey { get; private set; } // e.g. "user:123" or "anon:GUID"
        public List<CartItem> Items { get; private set; } = new();
        public DateTime LastUpdatedAt { get; private set; }

        protected Cart() { }

        public Cart(string cartKey, long? userId = null)
        {
            if (string.IsNullOrWhiteSpace(cartKey)) throw new ArgumentException("cartKey required", nameof(cartKey));
            CartKey = cartKey;
            UserId = userId;
            LastUpdatedAt = DateTime.Now;
        }

        public static Cart Create(string cartKey)
        {
            if (!Regex.IsMatch(cartKey, @"^user:\d+$"))
                throw new ArgumentException("Invalid CartKey!!!");

            return new Cart
            {
                CartKey = cartKey,
                CreationDate = DateTime.Now,
                LastUpdatedAt = DateTime.Now,
                Items = new(),
                UserId = long.Parse(cartKey[5..])
            };
        }

        public void AddItem(long productId, int qty)
        {
            if (qty <= 0) throw new ArgumentException("qty must be > 0", nameof(qty));

            var item = Items.FirstOrDefault(i => i.ProductId == productId);
            if (item == null)
            {
                Items.Add(new CartItem(productId, qty, CartKey));
            }
            else
            {
                item.IncreaseByQty(qty);
            }
            Touch();
        }

        public void UpdateItem(long productId, int qty)
        {
            if (qty <= 0) throw new ArgumentException("qty must be > 0", nameof(qty));

            var item = Items.FirstOrDefault(i => i.ProductId == productId);
            if (item == null) throw new InvalidOperationException("Item not found in cart");
            item.ChangeQuantity(qty);
            Touch();
        }

        public void IncreaseItem(long productId)
        {
            var item = Items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                item.Increase();
                Touch();
            }
        }

        public void DecreaseItem(long productId)
        {
            var item = Items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                item.Decrease();
                Touch();
            }
        }

        public void RemoveItem(long productId)
        {
            var item = Items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                Items.Remove(item);
                Touch();
            }
        }

        public void Clear()
        {
            Items.Clear();
            Touch();
        }

        public void Merge(Cart other)
        {
            if (other == null) return;
            foreach (var o in other.Items)
            {
                var existing = Items.FirstOrDefault(i => i.ProductId == o.ProductId);
                if (existing == null)
                    Items.Add(new CartItem(o.ProductId, o.Quantity, CartKey));
                else
                    existing.IncreaseByQty(o.Quantity);
            }
            Touch();
        }

        private void Touch() => LastUpdatedAt = DateTime.Now;
    }

}
