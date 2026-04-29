using _0_Framework.Application;
using Microsoft.Extensions.Logging;
using SiteManagement.Application.Contracts.SiteSetting;
using SiteManagement.Domain.SiteSettingAgg;
using System.Text.Json;

namespace SiteManagement.Application
{
    public class UpdateSiteSettingCommand
    {
        private readonly ISiteSettingRepository _repo;
        private readonly IFileUploader _fileUploader;
        private readonly ILogger<UpdateSiteSettingCommand> _logger;


        public UpdateSiteSettingCommand(
            ISiteSettingRepository repo,
            IFileUploader fileUploader,
            ILogger<UpdateSiteSettingCommand> logger)
        {
            _repo = repo;
            _fileUploader = fileUploader;
            _logger = logger;
        }

        public async Task<OperationResult> Execute(SiteSettingAdminDto dto, CancellationToken cancellation)
        {
            var op = new OperationResult();

            try
            {
                var setting = await _repo.GetAsync(cancellation);

                var data = JsonSerializer.Deserialize<SiteSettingDto>(setting.Data)
                           ?? new SiteSettingDto();

                // Upload Logo
                if (dto.Branding?.LogoFile != null)
                {
                    _fileUploader.DeleteFile(data.Logo);
                    data.Logo = await _fileUploader.UploadAsync(
                        dto.Branding.LogoFile,
                        "Site/logo",
                        cancellation);
                }

                // Upload Favicon
                if (dto.Branding?.FaviconFile != null)
                {
                    _fileUploader.DeleteFile(data.Favicon);
                    data.Favicon = await _fileUploader.UploadAsync(
                        dto.Branding.FaviconFile,
                        "Site/favicon",
                        cancellation);
                }

                // Update other sections
                data.Contact = dto.Contact;
                data.About = dto.About;
                data.Payment = dto.Payment;
                data.TicketTitles = dto.TicketTitles;
                data.TermsAndConditions = dto.TermsAndConditions;
                data.ReturnPolicy = dto.ReturnPolicy;
                data.Socials = dto.Socials;

                setting.Update(JsonSerializer.Serialize(data));
                await _repo.SaveChangesAsync(cancellation);

                return op.Succeeded("تنظیمات سایت با موفقیت به‌روز شد.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Site Setting Error");
                return op.Failed("خطا در ذخیره تنظیمات سایت.");
            }
        }
    }
}
