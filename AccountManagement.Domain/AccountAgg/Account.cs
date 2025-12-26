using _0_Framework.Domain;
using AccountManagement.Domain.RoleAgg;
using System.Reflection;

namespace AccountManagement.Domain.AccountAgg
{
    public class Account : BaseEntity
    {
        public string? Fullname { get; private set; }
        public string Username { get; private set; }
        public string Mobile { get; private set; }
        public string? Email { get; private set; }
        public string? Password { get; private set; }
        public string ProfilePhoto { get; private set; }
        public long RoleId { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsProfileCompleted { get; private set; } = true;

        // Navigation
        public Role? Role { get; private set; }

        // --- Constructor for creation ---
        public Account(string fullname, string username, string mobile, string email, string password, long roleId, string profilePhoto)
        {
            Fullname = fullname;
            Username = username;
            Mobile = mobile;
            Email = email;
            Password = password;
            RoleId = roleId;
            ProfilePhoto = profilePhoto;
            IsActive = true;
            IsProfileCompleted = true;
        }

        public Account(string mobile)
        {
            Fullname = null;
            Mobile = mobile;
            Username = mobile;
            Password = null;
            IsProfileCompleted = false;
            IsActive = true;
            RoleId = Roles.Customer;
        }

        // --- Behaviors (methods) ---
        public void Edit(string fullname, string username, string email, string mobile, long roleId, string profilePhoto)
        {
            Fullname = fullname;
            Username = username;
            Mobile = mobile;
            Email = email;
            RoleId = roleId;

            if (!string.IsNullOrWhiteSpace(profilePhoto))
                ProfilePhoto = profilePhoto;
        }

        public void CompleteProfile(string fullname,
            string username, string email, string password,
            string profilePhoto)
        {
            Fullname = fullname;
            Username = username;
            Email = email;
            Password = password;
            if (!string.IsNullOrWhiteSpace(profilePhoto))
                ProfilePhoto = profilePhoto;
            IsProfileCompleted = true;
        }

        public void ChangePassword(string newPassword)
        {
            Password = newPassword;
        }

        public void Activate() => IsActive = true;
        public void Deactivate() => IsActive = false;
        public void SetProfilePhoto(string photo) => ProfilePhoto = photo;
    }
}
