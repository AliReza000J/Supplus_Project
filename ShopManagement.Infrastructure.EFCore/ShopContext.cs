using Microsoft.EntityFrameworkCore;
using ShopManagement.Domain.CartAgg;
using ShopManagement.Domain.OrderAgg;
using ShopManagement.Domain.ProductAgg;
using ShopManagement.Domain.ProductCategoryAgg;
using ShopManagement.Domain.ProductPictureAgg;
using ShopManagement.Domain.SlideAgg;
using ShopManagement.Domain.UserProductInteractionAgg;
using ShopManagement.Infrastructure.EFCore.Mapping;

namespace ShopManagement.Infrastructure.EFCore
{
    public class ShopContext: DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<ProductPicture> ProductPictures { get; set; }
        public DbSet<Slide> Slides { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<ProductView> ProductViews { get; set; }
        public DbSet<UserFavoriteProduct> UserFavoriteProducts { get; set; }

        public ShopContext(DbContextOptions<ShopContext> options): base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var assembly = typeof(ProductCategoryMapping).Assembly;
            modelBuilder.ApplyConfigurationsFromAssembly(assembly);
            
            base.OnModelCreating(modelBuilder);
        }
    }
}
