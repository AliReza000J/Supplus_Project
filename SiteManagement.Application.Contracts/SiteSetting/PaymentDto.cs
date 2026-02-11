namespace SiteManagement.Application.Contracts.SiteSetting
{
    public class PaymentDto
    {
        public List<CardDto> Cards { get; set; } = new();
    }
}
