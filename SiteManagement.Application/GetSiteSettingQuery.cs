using SiteManagement.Application.Contracts.SiteSetting;
using SiteManagement.Domain.SiteSettingAgg;
using System.Text.Json;

namespace SiteManagement.Application
{
    public class GetSiteSettingQuery
    {
        private readonly ISiteSettingRepository _repo;

        public GetSiteSettingQuery(ISiteSettingRepository repo)
        {
            _repo = repo;
        }

        public async Task<SiteSettingDto> Execute(CancellationToken cancellation)
        {
            var setting = await _repo.GetAsync(cancellation);

            return JsonSerializer.Deserialize<SiteSettingDto>(setting.Data)
                   ?? new SiteSettingDto();
        }
    }
}
