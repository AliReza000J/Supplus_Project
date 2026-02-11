using _0_Framework.Domain;

namespace SiteManagement.Domain.SiteSettingAgg
{
    public interface ISiteSettingRepository: IRepository<long, SiteSetting>
    {
        Task<SiteSetting> GetAsync(CancellationToken cancellation);
    }
}