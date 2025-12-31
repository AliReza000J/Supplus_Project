namespace _01_PharmaPlusQuery.Contracts.Dashbord
{
    public interface IDashboardQuery
    {
        Task<DashboardKpiDto> GetKpisAsync(CancellationToken cancellation);
        Task<List<ChartDto>> GetMonthlySalesLineChartAsync(int months = 6, CancellationToken cancellation = default);
        Task<ChartDto> GetOrderStatusChartAsync(CancellationToken cancellation);
        Task<ChartDto> GetTopProductsDoughnutAsync(int take = 5, CancellationToken cancellation = default);
        Task<ComparisonChartDto> GetSalesComparisonAsync(CancellationToken cancellation);
        Task<ChartDto> GetConversionRateAsync(CancellationToken cancellation);
        Task<ChartDto> GetTopBrandsChartAsync(CancellationToken cancellation);
        Task<ChartDto> GetTopCategoriesChartAsync(CancellationToken cancellation);
    }
}
