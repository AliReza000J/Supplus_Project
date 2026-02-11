namespace AccountManagement.Domain.RoleAgg
{
    public static class Roles
    {
        public const long SuperAdmin = 1;
        public const long Admin = 2;
        public const long Support = 3;
        public const long ContentUploader = 4;
        public const long Customer = 5;
        public const long ColleagueUser = 10002;

        public static string GetRoleBy(long roleId)
        {
            return roleId switch
            {
                1 => "سوپر ادمین",
                2 => "ادمین",
                3 => "پشتیبان",
                4 => "تولیدکننده محتوا",
                5 => "کاربر سایت",
                10002 => "همکار",
                _ => "",
            };
        }
    }
}
