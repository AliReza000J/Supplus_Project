using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SiteManagement.Application.Contracts.SiteSetting;
using SiteManagement.Application;

namespace PharmaAdmin.Web.Areas.Admin.Pages.Site
{
    public class IndexModel : PageModel
    {
        private readonly GetSiteSettingQuery _query;
        private readonly UpdateSiteSettingCommand _command;

        public SiteSettingDto Setting { get; private set; }

        [BindProperty]
        public SiteSettingAdminDto Command { get; set; } = new();

        [TempData]
        public string Message { get; set; }

        public IndexModel(
            GetSiteSettingQuery query,
            UpdateSiteSettingCommand command)
        {
            _query = query;
            _command = command;
        }

        public async Task OnGet(CancellationToken cancellation)
        {
            Setting = await _query.Execute(cancellation);

            Command = new SiteSettingAdminDto
            {
                Contact = Setting.Contact ?? new ContactSettingDto(),
                Socials = Setting.Socials ?? new SocialsDto(),
                TicketTitles = Setting.TicketTitles ?? new TicketTitlesDto(),
                About = Setting.About ?? new AboutDto(),
                ReturnPolicy = Setting.ReturnPolicy ?? new ReturnPolicyDto(),
                TermsAndConditions = Setting.TermsAndConditions ?? new TermsAndConditionsDto(),
                Payment = Setting.Payment ?? new PaymentDto(),
                Branding = new BrandingSettingDto
                {
                    CurrentLogo = Setting.Logo,
                    CurrentFavicon = Setting.Favicon
                }
            };

            if (Command.Payment.Cards == null)
                Command.Payment.Cards = new List<CardDto>();

            if (Command.Socials.Items == null)
                Command.Socials.Items = new List<SocialItemDto>();

            if (Command.TicketTitles.Titles == null)
                Command.TicketTitles.Titles = new List<TicketTitleItem>();
        }

        public async Task<IActionResult> OnPost(CancellationToken cancellation)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            //foreach (var error in errors)
            //{
            //    Console.WriteLine(error.ErrorMessage);
            //}

            if (!ModelState.IsValid)
            {
                Setting = await _query.Execute(cancellation);
                return Page();
            }

            var result = await _command.Execute(Command, cancellation);

            TempData["Toast.Success"] = result.IsSucceeded;
            TempData["Toast.Message"] = result.Message;
            return RedirectToPage("./Index");
        }
    }

}
