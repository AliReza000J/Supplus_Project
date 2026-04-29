using _0_Framework.Domain;

namespace DiscountManagement.Domain.DiscountAgg
{
    public class DiscountUserUsage : BaseEntity
    {
        public long UserId { get; private set; }
        public int UsedCount { get; private set; }

        private DiscountUserUsage() { }

        public DiscountUserUsage(long userId)
        {
            UserId = userId;
            UsedCount = 0;
        }

        public void Increase()
        {
            UsedCount++;
        }
    }

}
