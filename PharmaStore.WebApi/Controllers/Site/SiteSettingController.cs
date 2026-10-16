using _0_Framework.Application;
using SiteManagement.Application;
using SiteManagement.Application.Contracts.SiteSetting;

namespace PharmaStore.WebApi.Controllers.Site
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/site-settings")]
    public class SiteSettingController : ControllerBase
    {
        private readonly GetSiteSettingQuery _query;

        public SiteSettingController(GetSiteSettingQuery query)
        {
            _query = query;
        }

        [HttpGet("global")]
        public async Task<ActionResult<ApiResponse<GlobalSettingResponse>>> GetGlobalSettings(CancellationToken cancellation)
        {
            var settings = await _query.Execute(cancellation);

            var response = new GlobalSettingResponse(
                settings.Logo,
                settings.Favicon,
                settings.Socials,
                settings.Contact,
                settings.GeneralSetting
            );

            return Ok(ApiResponse<GlobalSettingResponse>.Ok(response));
        }

        [HttpGet("about")]
        public async Task<ActionResult<ApiResponse<AboutDto>>> GetAbout(CancellationToken cancellation)
        {
            var settings = await _query.Execute(cancellation);
            return Ok(ApiResponse<AboutDto>.Ok(settings.About));
        }

        [HttpGet("payment")]
        public async Task<ActionResult<ApiResponse<PaymentDto>>> GetPaymentSettings(CancellationToken cancellation)
        {
            var settings = await _query.Execute(cancellation);
            return Ok(ApiResponse<PaymentDto>.Ok(settings.Payment));
        }

        [HttpGet("ticket-titles")]
        public async Task<ActionResult<ApiResponse<TicketTitlesDto>>> GetTicketTitles(CancellationToken cancellation)
        {
            var settings = await _query.Execute(cancellation);
            var titles = settings.TicketTitles.Titles.OrderBy(x => x.NumberOfRow).ToList();
            var result = new TicketTitlesDto
            {
                Titles = titles
            };
            return Ok(ApiResponse<TicketTitlesDto>.Ok(result));
        }
        [HttpGet("faq")]
        public async Task<ActionResult<ApiResponse<IEnumerable<FaqItemResponseDto>>>> GetFaq(CancellationToken cancellation)
        {
            var settings = await _query.Execute(cancellation);

            var faqListWithIds = settings.Faq.Items.Select((item, index) => new FaqItemResponseDto
            {
                FaqId = index + 1,
                Question = item.Question,
                Answer = item.Answer
            }).ToList();

            return Ok(ApiResponse<IEnumerable<FaqItemResponseDto>>.Ok(faqListWithIds));
        }


        [HttpGet("terms")]
        public async Task<ActionResult<ApiResponse<TermsAndConditionsDto>>> GetTerms(CancellationToken cancellation)
        {
            var settings = await _query.Execute(cancellation);
            return Ok(ApiResponse<TermsAndConditionsDto>.Ok(settings.TermsAndConditions));
        }

        [HttpGet("return-policy")]
        public async Task<ActionResult<ApiResponse<ReturnPolicyDto>>> GetReturnPolicy(CancellationToken cancellation)
        {
            var settings = await _query.Execute(cancellation);
            return Ok(ApiResponse<ReturnPolicyDto>.Ok(settings.ReturnPolicy));
        }
    }

    public record GlobalSettingResponse(
        string? Logo,
        string? Favicon,
        SocialsDto Socials,
        ContactSettingDto Contact,
        GeneralSettingDto GeneralSetting
    );
}
