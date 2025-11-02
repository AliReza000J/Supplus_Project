namespace _01_PharmaPlusQuery.Contracts.Product
{
    public interface IProductQuery
    {
        Task<ProductQueryModel> GetProductDetails(string productSlug);
        Task<List<ProductQueryModel>> GetLatestArrivals();
        Task<List<ProductQueryModel>> Search(string value);
        //List<CartItem> CheckInventoryStatus(List<CartItem> cartItems);
    }
}
