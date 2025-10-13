using _01_PharmaPlusQuery.Contracts.ProductCategory;
using Microsoft.EntityFrameworkCore;
using ShopManagement.Infrastructure.EFCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _01_PharmaPlusQuery.Query
{
    public class ProductCategoryQuery : IProductCategoryQuery
    {
        private readonly ShopContext _shopContext;

        public ProductCategoryQuery(ShopContext shopContext)
        {
            _shopContext = shopContext;
        }

        public async Task<List<ProductCategoryQueryModel>> GetProductCategories()
        {
            return await _shopContext.ProductCategories
                .Select(x => new ProductCategoryQueryModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Picture = x.Picture,
                    PictureAlt = x.PictureAlt,
                    PictureTitle = x.PictureTitle,
                    Slug = x.Slug
                }).AsNoTracking().ToListAsync();
        }

        public async Task<List<ProductCategoryQueryModel>> GetProductCategoriesWithProducts()
        {
            throw new NotImplementedException();
        }

        public async Task<ProductCategoryQueryModel> GetProductCategoryWithProducstsBy(string slug)
        {
            throw new NotImplementedException();
        }
    }
}
