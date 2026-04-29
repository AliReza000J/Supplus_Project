using _0_Framework.Application;

namespace ShopManagement.Application.Contracts.Order
{
    public class OrderSearchModel
    {
        public long? UserId { get; set; }
        public long? OrderId { get; set; }

        private string? _mobile;
        public string? Mobile
        {
            get => _mobile;
            set => _mobile = value?.ToEnglishNumber();
        }

        public int? Status { get; set; } = null;
        public int? PaymentMethod { get; set; }

        private string? _fromDate;
        public string? FromDate
        {
            get => _fromDate;
            set => _fromDate = value?.ToEnglishNumber();
        }

        private string? _toDate;
        public string? ToDate
        {
            get => _toDate;
            set => _toDate = value?.ToEnglishNumber();
        }
    }
}