namespace AccountManagement.Application.Contracts.Account
{
    public class AccountSearchModel
    {
        public string? Username { get; set; }
        public string? Mobile { get; set; }
        public long RoleId { get; set; }
        public bool? IsActive { get; set; }
    }
}