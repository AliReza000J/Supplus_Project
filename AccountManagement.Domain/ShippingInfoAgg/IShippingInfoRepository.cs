using _0_Framework.Domain;

namespace AccountManagement.Domain.ShippingInfoAgg
{
    public interface IShippingInfoRepository : IRepository<long, ShippingInfo>
    {
        Task<List<ShippingInfo>> GetAllByUserIdAsync(long userId, CancellationToken cancellation);
        void Delete(long id);
    }
}
