using _0_Framework.Domain;

namespace ShopManagement.Domain.OrderAgg
{
    public class Order : BaseEntity
    {
        public long AccountId { get; private set; }
        public DateTime LastModifiedDate { get; set; } = DateTime.Now;
        public int PayMethod { get; private set; }
        public string RecieverFullName { get; private set; }
        public string RecieverMobile { get; private set; }
        public string RecieverAddress { get; private set; }
        public string RecieverZipCode { get; private set; }
        public double TotalAmount { get; private set; }
        public double DiscountAmount { get; private set; }
        public double PayAmount { get; private set; }
        public int Status { get; private set; }
        public string? ShipTrackingNo { get; private set; }
        public string? CancellationReason { get; private set; }
        public string? PaymentRefId { get; private set; }
        public string? CardOwnerNationalCode { get; private set; }
        public string? CardOwnerFullname { get; private set; }
        public string? PaymentReceiptPic { get; private set; }
        public List<OrderItem> Items { get; private set; } = new();
        protected Order() { }

        public Order(long accountId, string fullName, string mobile,
            string address, string recieverZipCode, int payMethod, List<OrderItem> items)
        {
            if (accountId <= 0) throw new ArgumentException(nameof(accountId));
            if (items == null || !items.Any()) throw new ArgumentException("Order must have items", nameof(items));

            AccountId = accountId;
            RecieverFullName = fullName;
            RecieverMobile = mobile;
            RecieverAddress = address;
            PayMethod = payMethod;
            Items = items;
            TotalAmount = Items.Sum(i => i.Total());
            DiscountAmount = Items.Sum(i => i.TotalDiscAmount());
            PayAmount = Items.Sum(i => i.TotalPayAmount());
            Status = OrderStatus.Pending;
            RecieverZipCode = recieverZipCode;
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

    }
}