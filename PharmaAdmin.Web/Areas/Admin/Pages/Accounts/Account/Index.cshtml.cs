using _0_Framework.Application;
using _0_Framework.Infrastructure;
using AccountManagement.Application.Contracts.Account;
using AccountManagement.Application.Contracts.Account.ApplicationDTOs;
using AccountManagement.Application.Contracts.Role;
using AccountManagement.Configuration.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace PharmaAdmin.Web.Areas.Admin.Pages.Accounts.Account
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

        public IndexModel(IAccountApplication accountApplication,
            IRoleApplication roleApplication)
        {
            _roleApplication = roleApplication;
            _accountApplication = accountApplication;
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
            var command = new CreateAccount
            {
                Roles = (await _roleApplication.ListAsync(cancellation))
                .Where(x => x.Id != 1).ToList()
            };
            return Partial("./Create", command);
        }

        [NeedsPermission(AccountPermissions.CreateAccount)]
        public async Task<JsonResult> OnPostCreateAsync(CreateAccount command, CancellationToken cancellation)
        {
            var result = await _accountApplication.CreateAccountByAdmin(command, cancellation);
            TempData["Toast.Success"] = result.IsSucceeded;
            TempData["Toast.Message"] = result.Message;
            return new JsonResult(result);
        }

        [NeedsPermission(AccountPermissions.EditAccount)]
        public async Task<IActionResult> OnGetEditAsync(long id, CancellationToken cancellation)
        {
            var account = await _accountApplication.GetDetailsForAdmin(id, cancellation);

            account.Roles = (await _roleApplication.ListAsync(cancellation))
                .Where(x => x.Id != 1).ToList();
            return Partial("Edit", account);
        }

        [NeedsPermission(AccountPermissions.EditAccount)]
        public async Task<JsonResult> OnPostEditAsync(EditAccount command, CancellationToken cancellation)
        {
            var result = await _accountApplication.EditAccountByAdmin(command, cancellation);
            TempData["Toast.Success"] = result.IsSucceeded;
            TempData["Toast.Message"] = result.Message;
            return new JsonResult(result);
        }

        [NeedsPermission(AccountPermissions.ResetPassword)] 
        public IActionResult OnGetResetPassword(long id)
        {
            var command = new ResetUserPasswordViewModel { Id = id };
            return Partial("ResetPassword", command);
        }

        [NeedsPermission(AccountPermissions.ResetPassword)]
        public async Task<IActionResult> OnPostResetPasswordAsync(ResetUserPasswordViewModel command, CancellationToken cancellation)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _accountApplication.ResetPasswordByAdmin(command.Id, command.Password, cancellation);

            TempData["Toast.Success"] = result.IsSucceeded;
            TempData["Toast.Message"] = result.Message;
            return new JsonResult(result);
        }

        [NeedsPermission(AccountPermissions.DeactivateAccount)]
        public async Task<IActionResult> OnGetDeactivateAsync(long id, CancellationToken cancellation)
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

    public class ResetUserPasswordViewModel
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "کلمه عبور جدید را وارد کنید")]
        [PasswordValidation(ErrorMessage = "رمز عبور باید شامل حداقل یک حرف بزرگ، حداقل یک حرف کوچک و حداقل یک رقم، حداقل 6 کاراکتر و حداکثر 20 کاراکتر باشد")]
        public string Password { get; set; }

        [Required(ErrorMessage = "تکرار کلمه عبور را وارد کنید")]
        [Compare(nameof(Password), ErrorMessage = "کلمه عبور و تکرار آن مطابقت ندارند")]
        public string RePassword { get; set; }
    }

}