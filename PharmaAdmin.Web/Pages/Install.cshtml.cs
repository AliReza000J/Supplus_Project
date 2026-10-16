using AccountManagement.Application.Contracts.Account.ApplicationDTOs;
using AccountManagement.Application.Contracts.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PharmaShared.Web.Middlewares;

namespace PharmaAdmin.Web.Pages;

[AllowAnonymous]
public class InstallModel : PageModel
{
    private readonly IAccountApplication _application;
    private readonly IConfiguration _config;

    [TempData]
    public string Message { get; set; }

    public InstallModel(IAccountApplication application, IConfiguration config)
    {
        _application = application;
        _config = config;
    }

    [BindProperty]
    public SetupSuperAdminProfile Command { get; set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellation)
    {
        var needsSetup = await _application.NeedsSetup(cancellation);

        if (SetupState.IsSetupCompleted || !needsSetup)
        {
            SetupState.IsSetupCompleted = true;
            return NotFound();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellation)
    {
        if (!ModelState.IsValid)
            return Page();

        var expectedKey = _config["SetupConfig:InstallationKey"];
        if (Command.InstallationKey != expectedKey)
        {
            ModelState.AddModelError(string.Empty, "کلید نصب (Installation Key) نامعتبر است.");
            //Message = "کلید نصب (Installation Key) نامعتبر است.";
            return Page();
        }

        var admin = await _application.GetAccountDetails(Command.Id, cancellation);
        if (admin == null)
        {
            ModelState.AddModelError(string.Empty, "خطای بحرانی: کاربر ادمین اولیه در دیتابیس یافت نشد.");
            //Message = "خطای بحرانی: کاربر ادمین اولیه در دیتابیس یافت نشد.";
            return Page();
        }

        if (admin.IsCompleted)
        {
            SetupState.IsSetupCompleted = true;
            return BadRequest("سیستم قبلاً راه‌اندازی شده است.");
        }

        var result = await _application.Setup(Command, cancellation);

        if (result.IsSucceeded)
        {
            SetupState.IsSetupCompleted = true;
            TempData["Toast.Success"] = result.IsSucceeded;
            TempData["Toast.Message"] = result.Message;

            if (Request.Host.Host.Contains("localhost"))
            {
                return Redirect("https://localhost:5002/admin");
            }

            return Redirect("https://admin.supplus.ir/admin");
        }
        else
        {
            ModelState.AddModelError(string.Empty, result.Message);
            //Message = result.Message;
            return Page();
        }
    }
}
