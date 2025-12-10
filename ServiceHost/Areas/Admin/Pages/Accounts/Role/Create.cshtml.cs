using _0_Framework.Infrastructure;
using AccountManagement.Application.Contracts.Role;
using AccountManagement.Configuration.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ServiceHost.Areas.Admin.Pages.Accounts.Role
{
    public class CreateModel : PageModel
    {
        public CreateRole Command;
        private readonly IRoleApplication _roleApplication;

        public CreateModel(IRoleApplication roleApplication)
        {
            _roleApplication = roleApplication;
        }

        [NeedsPermission(AccountPermissions.CreateRole)]
        public void OnGet()
        {
        }

        [NeedsPermission(AccountPermissions.CreateRole)]
        public async Task<IActionResult> OnPost(CreateRole command)
        {
            var result = await _roleApplication.Create(command);
            return RedirectToPage("Index");
        }
    }
}