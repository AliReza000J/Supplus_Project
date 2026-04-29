using _0_Framework.Domain;
using _0_Framework.Application;

namespace ShopManagement.Domain.OrderAgg
{
    public class Order : BaseEntity
    {
        public long UserId { get; private set; }
        public DateTime LastModifiedDate { get; set; } = DateTime.Now;
        public int PayMethod { get; private set; }

        public string ReceiverFullName { get; private set; }
        public string ReceiverMobile { get; private set; }
        public string ReceiverAddress { get; private set; }
        public string ReceiverZipCode { get; private set; }

        public double TotalAmount { get; private set; }
        public double DiscountAmount { get; private set; }
        public double PayAmount { get; private set; }

        public string? CouponCode { get; set; }
        public long? AppliedCouponId { get; private set; }
        public double OrderCouponAmount { get; private set; }

        public int Status { get; private set; }
        public string? ShipTrackingNo { get; private set; }
        public string? CancellationReason { get; private set; }
        public string? PaymentRefId { get; private set; }
        public string? CardOwnerNationalCode { get; private set; }
        public string? CardOwnerFullname { get; private set; }
        public string? PaymentReceiptPic { get; private set; }
        public List<OrderItem> Items { get; private set; } = new();
        protected Order() { }

        public Order(long userId, string fullName, string mobile,
            string address, string receiverZipCode, int payMethod, List<OrderItem> items)
        {
            if (userId <= 0) throw new ArgumentException(nameof(userId));
            if (items == null || !items.Any()) throw new ArgumentException("Order must have items", nameof(items));

            UserId = userId;
            ReceiverFullName = fullName;
            ReceiverMobile = mobile;
            ReceiverAddress = address;
            PayMethod = payMethod;
            Items = items;
            TotalAmount = Items.Sum(i => i.Total());
            DiscountAmount = Items.Sum(i => i.TotalDiscAmount());
            PayAmount = Items.Sum(i => i.TotalPayAmount());
            Status = OrderStatus.Pending;
            ReceiverZipCode = receiverZipCode;
        }

        public void Touch() => LastModifiedDate = DateTime.Now;

        public void StartGatewayPayment(string trackId) // اضافه کردن ورودی
        {
            if (PayMethod != PaymentMethod.Gateway)
                throw new InvalidOperationException("این روش پرداخت Gateway نیست.");

            this.PaymentRefId = trackId; // ذخیره کد پیگیری موقت زیبال
            this.Status = OrderStatus.WaitingForGateway;
            Touch();
        }

        public void StartManualPayment()
        {
            if (PayMethod != PaymentMethod.CardToCard)
                throw new InvalidOperationException("این روش پرداخت کارت به کارت نیست.");

            Status = OrderStatus.WaitingForManualVerification; // وضعیت در انتظار تایید پرداخت کارت به کارت
            Touch();
        }

        public void MarkAsPaid(string paymentRefId)
        {
            if (string.IsNullOrWhiteSpace(paymentRefId)) throw new ArgumentException("paymentRefId is required!", nameof(paymentRefId));
            PaymentRefId = paymentRefId;
            Status = OrderStatus.Paid;
            Touch();
        }

        public void ConfirmManually()
        {
            Status = OrderStatus.Paid;
            Touch();
        }

        public void ProcessOrder()
        {
            Status = OrderStatus.Processing;
            Touch();
        }

        public void Ship(string trackingNo)
        {
            ShipTrackingNo = trackingNo;
            Status = OrderStatus.Shipped;
            Touch();
        }

        public void MarkAsDeliverd()
        {
            Status = OrderStatus.Delivered;
            Touch();
        }

        public void Cancel(string reason)
        {
            Status = OrderStatus.Cancelled;
            CancellationReason = reason; 
            Touch();
        }

        public void AddCardPaymentInfo(string cardOwnerName, string nationalCode, string receiptPic)
        {
            CardOwnerNationalCode = nationalCode;
            CardOwnerFullname = cardOwnerName;
            PaymentReceiptPic = receiptPic;
        }

        public void ApplyOrderCoupon(long couponId, string couponCode, double discountAmount)
        {
            this.AppliedCouponId = couponId;
            this.CouponCode = couponCode;
            this.OrderCouponAmount = discountAmount;
            this.PayAmount -= discountAmount;
        }
    }
}