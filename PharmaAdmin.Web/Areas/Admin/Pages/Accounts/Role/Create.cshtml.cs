using _0_Framework.Infrastructure;
using AccountManagement.Application.Contracts.Role;
using AccountManagement.Configuration.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PharmaAdmin.Web.Areas.Admin.Pages.Accounts.Role
{
    public class CreateModel : PageModel
    {
        public CreateRole? Command;
        private readonly IRoleApplication _roleApplication;

        public CreateModel(IRoleApplication roleApplication)
        {
            _roleApplication = roleApplication;
        }

        [NeedsPermission(AccountPermissions.CreateRole)]
        public Task OnGetAsync()
        {
            return Task.CompletedTask;
        }

        [NeedsPermission(AccountPermissions.CreateRole)]
        public async Task<IActionResult> OnPostAsync(CreateRole command, CancellationToken cancellation)
        {
            var result = await _roleApplication.Create(command, cancellation);
            TempData["Toast.Success"] = result.IsSucceeded;
            TempData["Toast.Message"] = result.Message;
            return RedirectToPage("Index");
        }
    }
}