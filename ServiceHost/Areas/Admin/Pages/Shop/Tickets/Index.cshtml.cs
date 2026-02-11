using _0_Framework.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SiteManagement.Application.Contracts.Ticket;

namespace ServiceHost.Areas.Admin.Pages.Shop.Tickets
{
    public class IndexModel : PageModel
    {
        [TempData]
        public string Message { get; set; }
        public List<TicketViewModel> Tickets;
        public TicketSearchModel SearchModel;

        private readonly ITicketApplication _ticketApplication;
        private readonly IAuthHelper _authHelper;

        public IndexModel(ITicketApplication ticketApplication, IAuthHelper authHelper)
        {
            _ticketApplication = ticketApplication;
            _authHelper = authHelper;
        }

        public async Task OnGet(TicketSearchModel searchModel, CancellationToken cancellation)
        {
            SearchModel = searchModel;
            Tickets = await _ticketApplication.Search(searchModel, cancellation);
        }

        public async Task<IActionResult> OnGetDetails(long id, CancellationToken cancellation)
        {
            var ticket = await _ticketApplication.GetDetailsAsync(id, cancellation);

            return Partial("./Reply", ticket);
        }


        public async Task<JsonResult> OnPostReply(ReplyTicket command, CancellationToken cancellation)
        {
            if (!ModelState.IsValid)
                return new JsonResult(new { IsSucceeded = false, Message = "ورودی نامعتبر است." });

            command.AdminId = _authHelper.CurrentUserId();

            var result = await _ticketApplication.ReplyTicket(command, cancellation);
            TempData["Toast.Success"] = result.IsSucceeded;
            TempData["Toast.Message"] = result.Message;
            return new JsonResult(result);
        }
    }
}
