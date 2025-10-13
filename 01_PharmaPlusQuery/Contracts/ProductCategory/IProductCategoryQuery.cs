using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _01_PharmaPlusQuery.Contracts.ProductCategory
{
    public interface IProductCategoryQuery
    {
        Task<ProductCategoryQueryModel> GetProductCategoryWithProducstsBy(string slug);
        Task<List<ProductCategoryQueryModel>> GetProductCategories();
        Task<List<ProductCategoryQueryModel>> GetProductCategoriesWithProducts();
    }
}