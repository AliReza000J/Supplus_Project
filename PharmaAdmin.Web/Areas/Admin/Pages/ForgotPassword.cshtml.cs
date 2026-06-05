using AccountManagement.Application.Contracts.Account;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PharmaAdmin.Web.Areas.Admin.Pages
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly IAccountApplication _accountApplication;

        public ForgotPasswordModel(IAccountApplication accountApplication)
        {
            _accountApplication = accountApplication;
        }

        [BindProperty]
        public SendPasswordResetLinkDto Command { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync(CancellationToken cancellation)
        {
            if (!ModelState.IsValid)
                return Page();

            var result = await _accountApplication.SendPasswordResetLink(Command, cancellation);

            if (result.IsSucceeded)
            {
                TempData["Toast.Success"] = true;
                TempData["Toast.Message"] = result.Message;
                return RedirectToPage("Login");
            }

            ErrorMessage = result.Message;
            return Page();
        }
    }
}