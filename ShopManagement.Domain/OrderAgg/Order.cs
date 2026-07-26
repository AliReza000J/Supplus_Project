using _0_Framework.Application;
using _0_Framework.Domain;
using System.ComponentModel.DataAnnotations;

namespace ShopManagement.Domain.OrderAgg
{
    public class Order : EntityBase
    {
        [Timestamp]
        public byte[] RowVersion { get; set; }

        public long UserId { get; private set; }
        public DateTime LastModifiedDate { get; set; } = DateTime.Now;
        public int PayMethod { get; private set; }

        public string ReceiverFullName { get; private set; }
        public string ReceiverMobile { get; private set; }
        public string ReceiverAddress { get; private set; }
        public string ReceiverZipCode { get; private set; }

        public double TotalAmount { get; private set; }
        public double DiscountAmount { get; private set; }
        public double ShippingCost { get; private set; }
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
            if (userId <= 0) throw new ArgumentException("Invalid UserId", nameof(userId));
            if (items == null || !items.Any()) throw new ArgumentException("Order must have items", nameof(items));

            UserId = userId;
            ReceiverFullName = fullName;
            ReceiverMobile = mobile;
            ReceiverAddress = address;
            ReceiverZipCode = receiverZipCode;
            PayMethod = payMethod;
            Items = items;
            TotalAmount = Items.Sum(i => i.Total());
            DiscountAmount = Items.Sum(i => i.TotalDiscAmount());
            PayAmount = Items.Sum(i => i.TotalPayAmount());
            Status = OrderStatus.Pending;

            Touch();
        }

        public void Touch() => LastModifiedDate = DateTime.Now;

        public void StartGatewayPayment(string trackId)
        {
            if (PayMethod != PaymentMethod.Gateway)
                throw new InvalidOperationException("این روش پرداخت Gateway نیست.");

            PaymentRefId = trackId;
            Status = OrderStatus.WaitingForGateway;
            Touch();
        }

        public void StartManualPayment()
        {
            if (PayMethod != PaymentMethod.CardToCard)
                throw new InvalidOperationException("این روش پرداخت کارت به کارت نیست.");

            Status = OrderStatus.WaitingForManualVerification;
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

        public void MarkAsNeedsRefund(string reason)
        {
            Status = OrderStatus.NeedsRefund;
            CancellationReason = reason;
            Touch();
        }

        public void RefundAndCancel ()
        {
            Status = OrderStatus.Refunded;
            CancellationReason += $"{Environment.NewLine}[تاریخ عودت]: {DateTime.Now.ToFarsiFull().ToPersianNumber()}";
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
            AppliedCouponId = couponId;
            CouponCode = couponCode;
            OrderCouponAmount = discountAmount;
            PayAmount -= discountAmount;
        }

        public void SetShippingCost(double shippingCost)
        {
            ShippingCost = shippingCost;
            PayAmount += shippingCost;
        }

        public void RequestReturn(string reason)
        {
            if (Status != OrderStatus.Delivered)
                throw new InvalidOperationException("فقط سفارش‌های تحویل داده شده می‌توانند مرجوع شوند.");

            Status = OrderStatus.ReturnRequested;

            CancellationReason = reason;
            Touch();
        }

        public void RejectReturn(string adminNotes)
        {
            if (Status != OrderStatus.ReturnRequested)
                throw new InvalidOperationException("این سفارش در وضعیت درخواست مرجوعی قرار ندارد.");

            Status = OrderStatus.Delivered;

            CancellationReason = adminNotes;
            Touch();
        }
    }
}