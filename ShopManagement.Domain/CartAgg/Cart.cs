using _0_Framework.Domain;

namespace ShopManagement.Domain.CartAgg
{
    public class Cart : BaseEntity
    {
        public long? AccountId { get; private set; } // user id
        public string CartKey { get; private set; } // e.g. "user:123" or "anon:GUID"
        public List<CartItem> Items { get; private set; } = new();
        public DateTime LastUpdatedAt { get; private set; }

        protected Cart() { }

        public Cart(string cartKey, long? accountId = null)
        {
            if (string.IsNullOrWhiteSpace(cartKey)) throw new ArgumentException("cartKey required", nameof(cartKey));
            CartKey = cartKey;
            AccountId = accountId;
            LastUpdatedAt = DateTime.Now;
        }

        public void AddItem(long productId, int qty, double unitPrice)
        {
            if (qty <= 0) throw new ArgumentException("qty must be > 0", nameof(qty));

            var item = Items.FirstOrDefault(i => i.ProductId == productId);
            if (item == null)
            {
                Items.Add(new CartItem(productId, qty, unitPrice));
            }
            else
            {
                item.Increase(qty);
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
                    Items.Add(new CartItem(o.ProductId, o.Quantity, o.UnitPrice));
                else
                    existing.Increase(o.Quantity);
            }
            Touch();
        }

        private void Touch() => LastUpdatedAt = DateTime.Now;
    }

}
