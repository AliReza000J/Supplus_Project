using _0_Framework.Domain;

namespace ShopManagement.Domain.OrderAgg
{
    public class Order : BaseEntity
    {
        public long AccountId { get; private set; }
        public int PaymentMethod { get; private set; }
        public string FullName { get; private set; } = "";
        public string Mobile { get; private set; } = "";
        public string Address { get; private set; } = "";
        public double TotalAmount { get; private set; }
        public double DiscountAmount { get; private set; }
        public double PayAmount { get; private set; }
        public OrderStatus Status { get; private set; }
        public string? TrackingNo { get; private set; }
        public string? PaymentRefId { get; private set; }
        public List<OrderItem> Items { get; private set; } = new();
        protected Order() { }

        public Order(long accountId, string fullName, string mobile, string address, List<OrderItem> items)
        {
            if (accountId <= 0) throw new ArgumentException(nameof(accountId));
            if (items == null || !items.Any()) throw new ArgumentException("Order must have items", nameof(items));

            AccountId = accountId;
            FullName = fullName;
            Mobile = mobile;
            Address = address;
            Items = items;
            TotalAmount = Items.Sum(i => i.Total());
            Status = OrderStatus.Pending;
        }

        public void MarkAsPaid(string paymentRefId)
        {
            if (string.IsNullOrWhiteSpace(paymentRefId)) throw new ArgumentException(nameof(paymentRefId));
            PaymentRefId = paymentRefId;
            Status = OrderStatus.Paid;
        }

        public void Ship(string trackingNo)
        {
            TrackingNo = trackingNo;
            Status = OrderStatus.Shipped;
        }

        public void Cancel()
        {
            Status = OrderStatus.Canceled;
        }

        public enum OrderStatus
        {
            Pending = 0,
            Paid = 1,
            Shipped = 2,
            Canceled = 3
        }
    }
}