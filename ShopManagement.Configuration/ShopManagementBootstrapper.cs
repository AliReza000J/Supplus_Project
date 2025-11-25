using _0_Framework.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShopManagement.Application;
using ShopManagement.Application.Contracts.Product;

//using ShopManagement.Application.Contracts.Order;
using ShopManagement.Application.Contracts.ProductCategory;
using ShopManagement.Domain.ProductAgg;

using ShopManagement.Application.Contracts.ProductPicture;
using ShopManagement.Application.Contracts.Slide;
//using ShopManagement.Domain.OrderAgg;
using ShopManagement.Domain.ProductCategoryAgg;
using ShopManagement.Domain.ProductPictureAgg;
//using ShopManagement.Domain.Services;
using ShopManagement.Domain.SlideAgg;
//using ShopManagement.Infrastructure.AccountAcl;
using ShopManagement.Infrastructure.EFCore;
using ShopManagement.Infrastructure.EFCore.Repository;
using _01_PharmaPlusQuery.Query;
using _01_PharmaPlusQuery.Contracts.Slide;
using _01_PharmaPlusQuery.Contracts.ProductCategory;
using _01_PharmaPlusQuery.Contracts.Product;
using ShopManagement.Configuration.Permissions;
//using ShopManagement.Infrastructure.InventoryAcl;

namespace ShopManagement.Configuration
{
    public class ShopManagementBootstrapper
    {
        public static void Configure(IServiceCollection services, string connectionString)
        {
            services.AddTransient<IProductCategoryApplication, ProductCategoryApplication>();
            services.AddTransient<IProductCategoryRepository, ProductCategoryRepository>();

            services.AddTransient<IProductApplication, ProductApplication>();
            services.AddTransient<IProductRepository, ProductRepository>();

            services.AddTransient<IProductPictureApplication, ProductPictureApplication>();
            services.AddTransient<IProductPictureRepository, ProductPictureRepository>();

            services.AddTransient<ISlideApplication, SlideApplication>();
            services.AddTransient<ISlideRepository, SlideRepository>();

            //services.AddTransient<IOrderRepository, OrderRepository>();
            //services.AddTransient<IOrderApplication, OrderApplication>();

            //services.AddSingleton<ICartService, CartService>();

            //services.AddTransient<IShopInventoryAcl, ShopInventoryAcl>();
            //services.AddTransient<IShopAccountAcl, ShopAccountAcl>();

            
            //services.AddTransient<ICartCalculatorService, CartCalculatorService>();

            // API
            services.AddTransient<ISlideQuery, SlideQuery>();
            services.AddTransient<IProductCategoryQuery, ProductCategoryQuery>();
            services.AddTransient<IProductQuery, ProductQuery>();


            services.AddTransient<IPermissionExposer, ShopPermissionExposer>();
            services.AddDbContext<ShopContext>(x => x.UseSqlServer(connectionString));
        }
    }
}