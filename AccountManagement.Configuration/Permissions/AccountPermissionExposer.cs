using _0_Framework.Infrastructure;

namespace AccountManagement.Configuration.Permissions
{
    public class AccountPermissionExposer : IPermissionExposer
    {
        public Dictionary<string, List<PermissionDto>> Expose()
        {
            return new Dictionary<string, List<PermissionDto>>
            {
                {
                    "accounts", new List<PermissionDto>
                    {
                        new PermissionDto(AccountPermissions.ListAccounts, "لیست کاربران"),
                        new PermissionDto(AccountPermissions.SearchAccounts, "جستجو کاربران"),
                        new PermissionDto(AccountPermissions.CreateAccount, "ایجاد کاربر"),
                        new PermissionDto(AccountPermissions.EditAccount, "ویرایش کاربر"),
                        new PermissionDto(AccountPermissions.DeleteAccount, "غبرفعال‌سازی کاربر"),
                        new PermissionDto(AccountPermissions.ActivateAccount, "غبرفعال‌سازی کاربر"),
                    }
                },
                {
                    "roles", new List<PermissionDto>
                    {
                        new PermissionDto(AccountPermissions.SearchRoles, "جستجو نقض‌ها"),
                        new PermissionDto(AccountPermissions.ListRoles, "لیست نقش‌ها"),
                        new PermissionDto(AccountPermissions.CreateRole, "ایجاد نقش"),
                        new PermissionDto(AccountPermissions.EditRole, "ویرایش نقش"),
                    }
                }
            };
        }
    }
}