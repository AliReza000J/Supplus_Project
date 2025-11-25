namespace _0_Framework.Application
{
    public class AuthViewModel
    {
        public long Id { get; set; }
        public long RoleId { get; set; }
        public string Fullname { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }          // optional, keep for compatibility
        public List<int> Permissions { get; set; } = new List<int>();

        public AuthViewModel()
        {
            Permissions = new List<int>();
        }

        public AuthViewModel(long id, long roleId, string fullname, string username, List<int> permissions)
        {
            Id = id;
            RoleId = roleId;
            Fullname = fullname;
            Username = username.ToLower();
            Permissions = permissions ?? new List<int>();
        }
    }
}