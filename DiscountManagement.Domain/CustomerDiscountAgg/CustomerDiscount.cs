using _0_Framework.Domain;

namespace DiscountManagement.Domain.CustomerDiscountAgg
{
    public class CustomerDiscount: BaseEntity
    {
        public long ProductId { get; private set; }
        public int DiscountRate { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        //public bool IsActive { get; set; }
        public int ProductCount { get; private set; }
        public string Reason { get; private set; }

        public CustomerDiscount(long productId, int discountRate, DateTime startDate,
            DateTime endDate, string reason, int productCount)
        {
            ProductId = productId;
            DiscountRate = discountRate;
            StartDate = startDate;
            EndDate = endDate;
            Reason = reason;
            ProductCount = productCount;
            //IsActive = true;
        }

        public void Edit(long productId, int discountRate, DateTime startDate,
            DateTime endDate, string reason, int productCount)
        {
            ProductId = productId;
            DiscountRate = discountRate;
            StartDate = startDate;
            EndDate = endDate;
            Reason = reason;
            ProductCount = productCount;
        }

        public void ReduceQty(int qty)
        {
            if (ProductCount >= qty)
                ProductCount -= qty;
        }
    }
}
