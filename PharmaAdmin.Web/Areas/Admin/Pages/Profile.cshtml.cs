using _0_Framework.Application;
using AccountManagement.Application.Contracts.Account;
using AccountManagement.Application.Contracts.Account.ApplicationDTOs;

namespace PharmaAdmin.Web.Areas.Admin.Pages
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;

    public class ProfileModel : PageModel
    {
        private readonly IAccountApplication _accountApplication;
        private readonly IAuthHelper _authHelper;

        public ProfileModel(IAccountApplication accountApplication, IAuthHelper authHelper)
        {
            _accountApplication = accountApplication;
            _authHelper = authHelper;
        }

        [BindProperty] public EditPersonalProfile EditCommand { get; set; } = new();
        [BindProperty] public EditSuperAdminProfile EditSuperAdminCommand { get; set; } = new();
        [BindProperty] public ChangePassword PasswordCommand { get; set; } = new();

        public bool IsSuperAdmin { get; set; }

        [TempData] public string Message { get; set; }

        public async Task OnGetAsync(CancellationToken cancellationToken)
        {
            var currentAccount = _authHelper.CurrentAccountInfo();
            IsSuperAdmin = currentAccount.Id == 1 && currentAccount.RoleId == 1;

            var details = await _accountApplication.GetDetailsForAdmin(currentAccount.Id, cancellationToken);

            if (details != null)
            {
                if (IsSuperAdmin)
                {
                    EditSuperAdminCommand = new EditSuperAdminProfile
                    {
                        Email = details.Email,
                        Mobile = details.Mobile
                    };
                }
                else
                {
                    EditCommand = new EditPersonalProfile
                    {
                        Id = details.Id,
                        Fullname = details.Fullname,
                        Username = details.Username,
                        Email = details.Email,
                        Mobile = details.Mobile
                    };

                    PasswordCommand = new ChangePassword { Id = currentAccount.Id };
                }
            }
        }

        public async Task<IActionResult> OnPostEditPersonalAsync(CancellationToken cancellationToken)
        {
            var currentAccount = _authHelper.CurrentAccountInfo();

            if (currentAccount.Id == 1)
                return RedirectToPage("./Profile");

            EditCommand.Id = currentAccount.Id;

            var result = await _accountApplication.EditPersonalProfile(EditCommand, cancellationToken);
            Message = result.Message;

            return RedirectToPage("./Profile");
        }

        public async Task<IActionResult> OnPostEditSuperAdminAsync(CancellationToken cancellationToken)
        {
            var currentAccount = _authHelper.CurrentAccountInfo();

            if (currentAccount.Id != 1)
                return RedirectToPage("./Profile");

            var result = await _accountApplication.EditSuperAdminProfile(EditSuperAdminCommand, cancellationToken);
            Message = result.Message;

            return RedirectToPage("./Profile");
        }

        public async Task<IActionResult> OnPostChangePasswordAsync(CancellationToken cancellationToken)
        {
            var currentUserId = _authHelper.CurrentUserId();

            if (currentUserId == 1)
                return RedirectToPage("./Profile");

            PasswordCommand.Id = currentUserId;

            var result = await _accountApplication.ChangePassword(PasswordCommand, currentUserId, cancellationToken);
            Message = result.Message;

            return RedirectToPage("./Profile");
        }
    }

}
