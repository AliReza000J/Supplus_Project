namespace SiteManagement.Application.Contracts.SiteSetting
{
    public class SiteSettingAdminDto
    {
        public ContactSettingDto Contact { get; set; } = new();
        public AboutDto About { get; set; } = new();
        public SocialsDto Socials { get; set; } = new();
        public TicketTitlesDto TicketTitles { get; set; } = new();
        public BrandingSettingDto Branding { get; set; } = new();
        public PaymentDto Payment { get; set; } = new();
        public TermsAndConditionsDto TermsAndConditions { get; set; } = new();
        public ReturnPolicyDto ReturnPolicy { get; set; } = new();
        public FaqDto Faq { get; set; } = new();
        public GeneralSettingDto GeneralSetting { get; set; } = new();
    }
}
