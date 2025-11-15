using _0_Framework.Application;
using AccountManagement.Application.Contracts.Account;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ServiceHost.Areas.Admin.Pages
{
    public class LoginModel : PageModel
    {
        [TempData]
        public string LoginMessage { get; set; }

        [TempData]
        public string RegisterMessage { get; set; }


        private readonly IAccountApplication _accountApplication;
        private readonly IAuthHelper _authHelper;

        public LoginModel(IAccountApplication accountApplication, IAuthHelper authHelper)
        {
            _accountApplication = accountApplication;
            _authHelper = authHelper;
        }

        public IActionResult OnGet()
        {
            if (_authHelper.IsAuthenticated())
                return RedirectToPage("/Index", new { area = "Admin" });

            return Page();
        }

        public IActionResult OnPostLogin(Login command)
        {
            if (_accountApplication.GetRoleIdByUsername(command.Username) != 2)
            {
                var result = _accountApplication.Login(command);
                if (result.IsSucceeded)
                    return RedirectToPage("/Index");

                LoginMessage = result.Message;
                return RedirectToPage("/Login");
            }

            return RedirectToPage("AccessDenied");
        }

        public IActionResult OnGetLogout()
        {
            _accountApplication.Logout();
            return RedirectToPage("/Login");
        }

    }
}
