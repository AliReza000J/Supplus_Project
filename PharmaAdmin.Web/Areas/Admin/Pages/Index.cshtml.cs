using _0_Framework.Application;
using _01_PharmaPlusQuery.Contracts.Dashbord;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PharmaAdmin.Web.Areas.Admin.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IAuthHelper _authHelper;
        private readonly IDashboardQuery _dashboardQuery;

        public DashboardKpiDto Kpis { get; private set; }
        public List<ChartDto> MonthlySalesChart { get; private set; }
        public ChartDto OrderStatusChart { get; private set; }
        public ChartDto TopProductsChart { get; private set; }
        public ChartDto ConversionRate { get; private set; }
        public ChartDto TopBrandsChart { get; private set; }
        public ChartDto TopCategoriesChart { get; private set; }
        public ComparisonChartDto SalesComparison { get; private set; }

        public IndexModel(IAuthHelper authHelper, IDashboardQuery dashboardQuery)
        {
            _authHelper = authHelper;
            _dashboardQuery = dashboardQuery;
        }

        public async Task<IActionResult> OnGet(CancellationToken cancellation)
        {
            var currentAccout = _authHelper.CurrentAccountInfo();

            if (currentAccout == null && !_authHelper.IsAuthenticated())
                return RedirectToPage("/Login");

            await LoadDashboardAsync(cancellation);
            return Page();
        }

        private async Task LoadDashboardAsync(CancellationToken cancellation)
        {
            Kpis = await _dashboardQuery.GetKpisAsync(cancellation);
            MonthlySalesChart = await _dashboardQuery.GetMonthlySalesLineChartAsync(6, cancellation);
            OrderStatusChart = await _dashboardQuery.GetOrderStatusChartAsync(cancellation);
            TopProductsChart = await _dashboardQuery.GetTopProductsDoughnutAsync(5, cancellation);
            ConversionRate = await _dashboardQuery.GetConversionRateAsync(cancellation);
            SalesComparison = await _dashboardQuery.GetSalesComparisonAsync(cancellation);
            TopBrandsChart = await _dashboardQuery.GetTopBrandsChartAsync(cancellation);
            TopCategoriesChart = await _dashboardQuery.GetTopCategoriesChartAsync(cancellation);
        }
    }
}
