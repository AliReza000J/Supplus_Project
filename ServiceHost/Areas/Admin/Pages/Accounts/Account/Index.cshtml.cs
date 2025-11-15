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

        public IndexModel(IAccountApplication accountApplication, IRoleApplication roleApplication, IAuthHelper authHelper)
        {
            _roleApplication = roleApplication;
            _accountApplication = accountApplication;
            _authHelper = authHelper;
        }

        public void OnGet(AccountSearchModel searchModel)
        {
            Roles = new SelectList(_roleApplication.List(), "Id", "Name");
            Accounts = _accountApplication.Search(searchModel);
        }

        [NeedsPermission(AccountPermissions.CreateAccount)]
        public IActionResult OnGetCreate()
        {
            var command = new RegisterAccount
            {
                Roles = _roleApplication.List()
            };
            return Partial("./Create", command);
        }

        [NeedsPermission(AccountPermissions.CreateAccount)]
        public JsonResult OnPostCreate(RegisterAccount command)
        {
            var result = _accountApplication.Register(command);
            return new JsonResult(result);
        }

        [NeedsPermission(AccountPermissions.EditAccount)]
        public IActionResult OnGetEdit(long id)
        {
            var account = _accountApplication.GetDetails(id);
            account.Roles = _roleApplication.List();
            return Partial("Edit", account);
        }

        [NeedsPermission(AccountPermissions.EditAccount)]
        public JsonResult OnPostEdit(EditAccount command)
        {
            var userId = _authHelper.CurrentAccountId();
            var result = _accountApplication.Edit(command, userId.ToString());
            return new JsonResult(result);
        }

        public IActionResult OnGetChangePassword(long id)
        {
            var command = new ChangePassword { Id = id };
            return Partial("ChangePassword", command);
        }

        public JsonResult OnPostChangePassword(ChangePassword command)
        {
            var userId = _authHelper.CurrentAccountId();
            var result = _accountApplication.ChangePassword(command, userId.ToString());
            return new JsonResult(result);
        }

        [NeedsPermission(AccountPermissions.DeleteAccount)]
        public IActionResult OnGetDelete(long id)
        {
            var result = _accountApplication.Delete(id);
            if (result.IsSucceeded)
                return RedirectToPage();

            Message = result.Message;
            return RedirectToPage();
        }

        [NeedsPermission(AccountPermissions.ActivateAccount)]
        public IActionResult OnGetActivate(long id)
        {
            var result = _accountApplication.Activate(id);
            if (result.IsSucceeded)
                return RedirectToPage();

            Message = result.Message;
            return RedirectToPage();
        }
    }
}
