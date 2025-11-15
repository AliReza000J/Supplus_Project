using _0_Framework.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ServiceHost.Areas.Admin.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IAuthHelper _authHelper;

        public IndexModel(IAuthHelper authHelper)
        {
            _authHelper = authHelper;
        }

        public IActionResult OnGet()
        {
            var currentAccout = _authHelper.CurrentAccountInfo();

            if (currentAccout == null && !_authHelper.IsAuthenticated())
                return RedirectToPage("/Login");

            return Page();
        }
    }
}
