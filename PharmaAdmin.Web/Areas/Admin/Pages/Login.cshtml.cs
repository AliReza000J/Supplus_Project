using _0_Framework.Application;
using AccountManagement.Application.Contracts.Account;
using AccountManagement.Application.Contracts.Account.ApplicationDTOs;
using AccountManagement.Domain.RoleAgg;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PharmaShared.Web.Filters;
using SiteManagement.Application;

namespace PharmaAdmin.Web.Areas.Admin.Pages
{
    [RateLimit(Limit = 10, Seconds = 300)]
    public class LoginModel : PageModel
    {
        [TempData]
        public string LoginMessage { get; set; }

        [TempData]
        public string Logo { get; set; }

        private readonly IAccountApplication _accountApplication;
        private readonly IAuthHelper _authHelper;
        private readonly GetLogoFromSetting _getLogoCommand;

        public LoginModel(IAccountApplication accountApplication, IAuthHelper authHelper, GetLogoFromSetting getLogoCommand)
        {
            _accountApplication = accountApplication;
            _authHelper = authHelper;
            _getLogoCommand = getLogoCommand;
        }

        public async Task<IActionResult> OnGet()
        {
            if (_authHelper.IsAuthenticated())
                return RedirectToPage("/Index", new { area = "Admin" });

            Logo = await _getLogoCommand.Execute(CancellationToken.None);

            return Page();
        }

        public async Task<IActionResult> OnPostLoginAsync(LoginViaUserPass command, CancellationToken cancellation)
        {
            var roleId = await _accountApplication.GetRoleIdByUsername(command.Username, cancellation);
            if (roleId != Roles.Customer)
            {
                var result = await _accountApplication.LoginToAdminPanel(command, cancellation);
                if (result.IsSucceeded)
                {
                    TempData["Toast.Success"] = result.IsSucceeded;
                    TempData["Toast.Message"] = $"{command.Username} عزیز خوش آمدید!";
                    return RedirectToPage("/Index");
                }

                LoginMessage = result.Message;
                return RedirectToPage("/Login");
            }

            return RedirectToPage("AccessDenied");
        }

        public async Task<IActionResult> OnGetLogout(CancellationToken cancellation = default)
        {
            await _accountApplication.LogoutFromAdminPanel(cancellation);
            return RedirectToPage("/Login");
        }

    }
}
