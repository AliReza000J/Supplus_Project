using SiteManagement.Application.Contracts.SiteSetting;
using SiteManagement.Domain.SiteSettingAgg;
using System.Text.Json;

namespace SiteManagement.Application
{
    public class GetLogoFromSetting
    {
        private readonly ISiteSettingRepository _repo;

        public GetLogoFromSetting(ISiteSettingRepository repo)
        {
            _repo = repo;
        }
        public async Task<string> Execute(CancellationToken cancellation)
        {
            var setting = await _repo.GetAsync(cancellation);

            var data = JsonSerializer.Deserialize<SiteSettingDto>(setting.Data)
                           ?? new SiteSettingDto();

            return data.Logo ?? "";
        }
    }
}
