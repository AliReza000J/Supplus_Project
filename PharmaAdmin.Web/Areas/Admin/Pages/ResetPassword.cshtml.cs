using AccountManagement.Application.Contracts.Account;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PharmaAdmin.Web.Areas.Admin.Pages
{
    public class ResetPasswordModel : PageModel
    {
        private readonly IAccountApplication _accountApplication;

        public ResetPasswordModel(IAccountApplication accountApplication)
        {
            _accountApplication = accountApplication;
        }

        [BindProperty]
        public ResetPasswordWithLinkDto Command { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public IActionResult OnGet(string email, string token)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
                return RedirectToPage("Login");

            Command = new ResetPasswordWithLinkDto
            {
                Email = email,
                Token = token
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(CancellationToken cancellation)
        {
            if (!ModelState.IsValid)
                return Page();

            var result = await _accountApplication.ResetPasswordWithLink(Command, cancellation);

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