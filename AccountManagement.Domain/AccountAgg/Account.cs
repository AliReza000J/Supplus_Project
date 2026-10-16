using _0_Framework.Domain;
using AccountManagement.Domain.RoleAgg;

namespace AccountManagement.Domain.AccountAgg
{
    public class Account : EntityBase
    {
        public string? Fullname { get; private set; }
        public string Username { get; private set; }
        public string Mobile { get; private set; }
        public string? Email { get; private set; }
        public string? Password { get; private set; }
        public string ProfilePhoto { get; private set; }
        public long RoleId { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsProfileCompleted { get; private set; } = true; // password and fullname for users - setup for admin

        // Navigation
        public Role? Role { get; private set; }

        // --- EF Core Constructor ---
        protected Account() { }

        // --- Constructor for Full Creation (Admin Panel) ---
        public Account(string fullname, string username, string mobile, string? email, string password, long roleId, string profilePhoto)
        {
            Fullname = fullname;
            Username = username.ToLower();
            Mobile = mobile;
            Email = string.IsNullOrWhiteSpace(email) ? null : email.ToLower();
            Password = password;
            RoleId = roleId;
            ProfilePhoto = profilePhoto;
            IsActive = true;
            IsProfileCompleted = true;
        }

        // --- Constructor for Quick Creation (Store API - OTP) ---
        public Account(string mobile, string defaultPhoto, long customerRoleId = 5)
        {
            Mobile = mobile;

            string mobileStr = Mobile;
            string last4Digits = mobileStr.Length >= 4 ? mobileStr.Substring(mobileStr.Length - 4) : mobileStr;
            Username = $"user{last4Digits}";

            Fullname = null;
            Password = null;
            Email = null;
            IsProfileCompleted = false;
            IsActive = true;
            RoleId = customerRoleId;
            ProfilePhoto = defaultPhoto;
        }

        public Account(string fullname, string username, string profilePhoto, int roleId)
        {
            Fullname = fullname;
            Username = username;
            ProfilePhoto = profilePhoto;
            RoleId = roleId;
            Password = null;
            Email = null;
            Mobile = "09120000000";
            IsActive = true;
            IsProfileCompleted = false;
        }

        // --- Behaviors (Methods) ---

        public void EditFullInfoByAdmin(string fullname, string username, string? email, string mobile, long roleId, string profilePhoto)
        {
            if (Id == 1 || this.Username.ToLower() == "admin")
            {
                Fullname = "admin";
                Username = "admin";
                RoleId = 1;
            }
            else
            {
                Fullname = fullname;
                Username = username.ToLower();
                RoleId = roleId;
            }

            Mobile = mobile;
            Email = string.IsNullOrWhiteSpace(email) ? null : email.ToLower();

            if (!string.IsNullOrWhiteSpace(profilePhoto))
                ProfilePhoto = profilePhoto;
        }

        public void UpdatePersonalProfileAndMobileAdminPanel(string fullname, string username, string mobile, string? email, string profilePhoto)
        {
            if (Id == 1 || this.Username.ToLower() == "admin")
            {
                Fullname = "admin";
                Username = "admin";
            }
            else
            {
                Fullname = fullname;
                Username = username.ToLower();
            }

            Mobile = mobile;
            Email = string.IsNullOrWhiteSpace(email) ? null : email.ToLower();

            if (!string.IsNullOrWhiteSpace(profilePhoto))
                ProfilePhoto = profilePhoto;
        }

        public void UpdateProfileInfoApi(string fullname, string username, string? email, string profilePhoto)
        {
            if (Id == 1 || this.Username.ToLower() == "admin")
            {
                Fullname = "admin";
                Username = "admin";
            }
            else
            {
                Fullname = fullname;
                Username = username.ToLower();
            }

            Email = string.IsNullOrWhiteSpace(email) ? null : email.ToLower();

            if (!string.IsNullOrWhiteSpace(profilePhoto))
                ProfilePhoto = profilePhoto;

            IsProfileCompleted = !string.IsNullOrWhiteSpace(Password);
        }

        public void UpdateSuperAdmin(string email, string mobile)
        {
            Email = email.ToLower();
            Mobile = mobile;
        }

        public void SetPassword(string password)
        {
            Password = password;

            IsProfileCompleted = !string.IsNullOrWhiteSpace(Fullname);
        }

        public void SetProfilePhoto(string path)
        {
            ProfilePhoto = path;
        }

        public void ChangePassword(string newPassword)
        {
            Password = newPassword;
        }

        public void ChangeMobile(string newMobile)
        {
            Mobile = newMobile;
        }

        public void SetCompleted() => IsProfileCompleted = true;

        public void Activate() => IsActive = true;

        public void Deactivate()
        {
            if (Id == 1 || Username.ToLower() == "admin")
                return;

            IsActive = false;
        }

        public bool IsCompleted() => IsProfileCompleted;
    }
}
