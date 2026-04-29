using _0_Framework.Application;
using Microsoft.AspNetCore.Mvc;
using SiteManagement.Application;
using SiteManagement.Application.Contracts.SiteSetting;

namespace PharmaStore.WebApi.Controllers.Site
{
    [ApiController]
    [Route("api/site-settings")]
    public class SiteSettingController : ControllerBase
    {
        private readonly GetSiteSettingQuery _query;

        public SiteSettingController(GetSiteSettingQuery query)
        {
            _query = query;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<SiteSettingDto>>> Get(CancellationToken cancellation)
        {
            var result = await _query.Execute(cancellation);
            return Ok(ApiResponse<SiteSettingDto>.Ok(result));
        }
    }

}
