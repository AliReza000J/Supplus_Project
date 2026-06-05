using _0_Framework.Infrastructure;
using AccountManagement.Application.Contracts.Role;
using AccountManagement.Configuration.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PharmaAdmin.Web.Areas.Admin.Pages.Accounts.Role
{
    public class EditModel : PageModel
    {
        public EditRole? Command;
        public List<SelectListItem> Permissions = new List<SelectListItem>();
        private readonly IRoleApplication _roleApplication;
        private readonly IEnumerable<IPermissionExposer> _exposers;

        public EditModel(IRoleApplication roleApplication, IEnumerable<IPermissionExposer> exposers)
        {
            _roleApplication = roleApplication;
            _exposers = exposers;
        }

        [NeedsPermission(AccountPermissions.EditRole)]
        public async Task<IActionResult> OnGetAsync(long id, CancellationToken cancellation)
        {
            if (id == 1)
                return RedirectToPage("/AccessDenied");

            var groups = new Dictionary<string, SelectListGroup>();
            Command = await _roleApplication.GetDetails(id, cancellation);
            foreach (var exposer in _exposers)
            {
                var exposedPermissions = exposer.Expose();

                foreach (var (key, value) in exposedPermissions)
                {
                    if (!groups.ContainsKey(key))
                        groups[key] = new SelectListGroup { Name = key };

                    var group = groups[key];

                    foreach (var permission in value)
                    {
                        var item = new SelectListItem(permission.Name, permission.Code.ToString())
                        {
                            Group = group
                        };

                        if (Command.MappedPermissions.Any(x => x.Code == permission.Code))
                            item.Selected = true;

                        Permissions.Add(item);
                    }
                }
            }

            return Page();
        }

        [NeedsPermission(AccountPermissions.EditRole)]
        public async Task<IActionResult> OnPostAsync(EditRole command, CancellationToken cancellation)
        {
            var result = await _roleApplication.Edit(command, cancellation);
            if (result.IsSucceeded)
            {
                TempData["Toast.Success"] = "true";
                TempData["Toast.Message"] = result.Message;
                return RedirectToPage("./Index");
            }

            TempData["Toast.Success"] = "false";
            TempData["Toast.Message"] = result.Message;
            return Page();
        }
    }
}