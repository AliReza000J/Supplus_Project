namespace AccountManagement.Domain.RoleAgg
{
    public static class Roles
    {
        public const long Admin = 1;
        public const long Customer = 2;
        public const long Support = 3;
        public const long ContentUploader = 4;
        public const long ColleagueUser = 10002;

        public static string GetRoleBy(long roleId)
        {
            return roleId switch
            {
                1 => "ادمین",
                2 => "کاربر سایت",
                3 => "فروشنده",
                4 => "تولیدکننده محتوا",
                10002 => "همکار",
                _ => "",
            };
        }
    }
}
