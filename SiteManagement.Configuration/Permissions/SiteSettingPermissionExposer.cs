using _0_Framework.Infrastructure;

namespace SiteManagement.Infrastructure.Configuration.Permissions
{
    public class SiteSettingPermissionExposer : IPermissionExposer
    {
        public Dictionary<string, List<PermissionDto>> Expose()
        {
            return new Dictionary<string, List<PermissionDto>>
            {
                {
                    "site-settings", new List<PermissionDto>
                    {
                        new PermissionDto(SiteSettingPermissions.Read, "مشاهده تنظیمات سایت"),
                        new PermissionDto(SiteSettingPermissions.Write, "اعمال تغییر در تنظیمات سایت"),
                    }
                }
            };
        }
    }
}