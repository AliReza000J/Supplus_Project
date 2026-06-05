using _0_Framework.Infrastructure;
using Microsoft.EntityFrameworkCore;
using SiteManagement.Domain.SiteSettingAgg;

namespace SiteManagement.Infrastructure.EFCore.Repository
{
    public class SiteSettingRepository : RepositoryBase<long, SiteSetting>, ISiteSettingRepository
    {
        private readonly SiteContext _context;
        public SiteSettingRepository(SiteContext context) : base(context)
        {
            _context = context;
        }

        public async Task<SiteSetting> GetAsync(CancellationToken cancellation)
        {
            var setting = await _context.SiteSettings
                .OrderBy(x => x.Id).FirstOrDefaultAsync(cancellation);

            if (setting == null)
            {
                setting = SiteSetting.CreateDefault();
                _context.SiteSettings.Add(setting);
                await _context.SaveChangesAsync(cancellation);
            }

            return setting;
        }
    }
}
