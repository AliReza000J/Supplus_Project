namespace SiteManagement.Application.Contracts.SiteSetting
{
    public class GeneralSettingDto
    {
        public bool IsMaintenanceMode { get; set; } = false;
        public string? CopyrightText { get; set; }
    }
}