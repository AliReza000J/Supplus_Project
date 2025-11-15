namespace ShopManagement.Domain.Services
{
    // IProductPricingService: returns price, discount and snapshot fields for a product
    public interface IProductPricingService
    {
        /// <summary>
        /// Get current unit price and discount data for product
        /// </summary>
        Task<(double UnitPrice, double DiscountAmount, int DiscountRate, int DiscountApplied,  string Name, string Picture)> GetPricingAsync(long productId);
    }
}
