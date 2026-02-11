namespace SiteManagement.Application.Contracts.SiteSetting
{
    public class SiteSettingDto
    {
        public ContactSettingDto Contact { get; set; } = new();
        public AboutDto About { get; set; } = new();
        public SocialsDto Socials { get; set; } = new();
        public PaymentDto Payment { get; set; } = new();
        public string? Logo { get; set; }
        public string? Favicon { get; set; }
        public TermsAndConditionsDto TermsAndConditions { get; set; } = new();
        public ReturnPolicyDto ReturnPolicy { get; set; } = new();

    }
}
