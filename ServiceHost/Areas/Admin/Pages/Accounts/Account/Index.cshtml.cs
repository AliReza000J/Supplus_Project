using _0_Framework.Application;
using _0_Framework.Infrastructure;
using AccountManagement.Application.Contracts.Account;
using AccountManagement.Application.Contracts.Role;
using AccountManagement.Configuration.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ServiceHost.Areas.Admin.Pages.Accounts.Account
{
    public class IndexModel : PageModel
    {
        [TempData]
        public string Message { get; set; }

        public AccountSearchModel SearchModel;
        public List<AccountViewModel> Accounts;
        public SelectList Roles;

        private readonly IRoleApplication _roleApplication;
        private readonly IAccountApplication _accountApplication;
        private readonly IAuthHelper _authHelper;

        public IndexModel(IAccountApplication accountApplication,
            IRoleApplication roleApplication, IAuthHelper authHelper)
        {
            _roleApplication = roleApplication;
            _accountApplication = accountApplication;
            _authHelper = authHelper;
        }

        public async Task OnGetAsync(AccountSearchModel searchModel, CancellationToken cancellation)
        {
            Roles = new SelectList((await _roleApplication.ListAsync(cancellation))
                .Where(x => x.Id != 1).ToList(), "Id", "Name");
            Accounts = await _accountApplication.Search(searchModel, cancellation);
        }

        [NeedsPermission(AccountPermissions.CreateAccount)]
        public async Task<IActionResult> OnGetCreateAsync(CancellationToken cancellation)
        {
            var command = new RegisterAccount
            {
                Roles = (await _roleApplication.ListAsync(cancellation))
                .Where(x => x.Id != 1).ToList()
            };
            return Partial("./Create", command);
        }

        [NeedsPermission(AccountPermissions.CreateAccount)]
        public async Task<JsonResult> OnPostCreateAsync(RegisterAccount command, CancellationToken cancellation)
        {
            var result = await _accountApplication.Register(command, cancellation);
            TempData["Toast.Success"] = result.IsSucceeded;
            TempData["Toast.Message"] = result.Message;
            return new JsonResult(result);
        }

        [NeedsPermission(AccountPermissions.EditAccount)]
        public async Task<IActionResult> OnGetEditAsync(long id, CancellationToken cancellation)
        {
            var account = await _accountApplication.GetDetails(id, cancellation);
            account.Roles = (await _roleApplication.ListAsync(cancellation))
                .Where(x => x.Id != 1).ToList();
            return Partial("Edit", account);
        }

        [NeedsPermission(AccountPermissions.EditAccount)]
        public async Task<JsonResult> OnPostEditAsync(EditAccount command, CancellationToken cancellation)
        {
            var userId = _authHelper.CurrentUserId();
            var result = await _accountApplication.Edit(command, userId, cancellation);
            TempData["Toast.Success"] = result.IsSucceeded;
            TempData["Toast.Message"] = result.Message;
            return new JsonResult(result);
        }

        public IActionResult OnGetChangePassword(long id)
        {
            var command = new ChangePassword { Id = id };
            return Partial("ChangePassword", command);
        }

        public async Task<IActionResult> OnPostChangePasswordAsync(ChangePassword command, CancellationToken cancellation)
        {
            var userId = _authHelper.CurrentUserId();
            var result = await _accountApplication.ChangePassword(command, userId, cancellation);
            TempData["Toast.Success"] = result.IsSucceeded;
            TempData["Toast.Message"] = result.Message;
            return new JsonResult(result);
        }

        [NeedsPermission(AccountPermissions.DeleteAccount)]
        public async Task<IActionResult> OnGetDeleteAsync(long id, CancellationToken cancellation)
        {
            if (id == 1)
                return RedirectToPage();

            var result = await _accountApplication.Deactivate(id, cancellation);
            TempData["Toast.Success"] = result.IsSucceeded;
            TempData["Toast.Message"] = result.Message;
            return RedirectToPage();
        }

        [NeedsPermission(AccountPermissions.ActivateAccount)]
        public async Task<IActionResult> OnGetActivateAsync(long id, CancellationToken cancellation)
        {
            if (id == 1)
                return RedirectToPage();

            var result = await _accountApplication.Activate(id, cancellation);
            TempData["Toast.Success"] = result.IsSucceeded;
            TempData["Toast.Message"] = result.Message;
            return RedirectToPage();
        }
    }
}
