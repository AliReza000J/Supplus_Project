namespace AccountManagement.Application.Contracts.Account.ApplicationDTOs
{
    public class AccountViewModel
    {
        public long Id { get; set; }
        public string? Fullname { get; set; }
        public string? Username { get; set; }
        public string Mobile { get; set; }
        public string? Email { get; set; }
        public long RoleId { get; set; }
        public string RoleName { get; set; }
        public string ProfilePhoto { get; set; }
        public bool IsActive { get; set; }
        public bool IsCompleted { get; set; }
        public bool HasPassword { get; set; }
        public string CreationDate { get; set; }
    }
}
