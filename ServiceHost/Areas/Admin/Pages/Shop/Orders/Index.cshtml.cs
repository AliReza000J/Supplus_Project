using AccountManagement.Application.Contracts.Account;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using ShopManagement.Application.Contracts.Order;

namespace ServiceHost.Areas.Admin.Pages.Shop.Orders
{
    public class IndexModel : PageModel
    {
        public OrderSearchModel? SearchModel;
        public SelectList? Accounts;
        public SelectList? Statuses;
        public List<OrderViewModel> Orders;

        private readonly IOrderApplication _orderApplication;
        private readonly IAccountApplication _accountApplication;

        public IndexModel(IOrderApplication orderApplication, IAccountApplication accountApplication)
        {
            _orderApplication = orderApplication;
            _accountApplication = accountApplication;
        }

        public async Task OnGet(OrderSearchModel searchModel)
        {
            Accounts = new SelectList(await _accountApplication.GetAccounts(), "Id", "Fullname");
            Statuses = new SelectList(_orderApplication.GetStatuses(), "Id", "Name");
            Orders = await _orderApplication.SearchAsync(searchModel);
        }

        public async Task<IActionResult> OnGetItems(long id)
        {
            var items = await _orderApplication.GetItems(id);
            return Partial("Items", items);
        }
    }
}