using _0_Framework.Infrastructure;

namespace ShopManagement.Configuration.Permissions
{
    public class ShopPermissionExposer : IPermissionExposer
    {
        public Dictionary<string, List<PermissionDto>> Expose()
        {
            return new Dictionary<string, List<PermissionDto>>
            {
                {
                    "products", new List<PermissionDto>
                    {
                        new PermissionDto(ShopPermissions.ListProducts, "لیست محصولات"),
                        new PermissionDto(ShopPermissions.SearchProducts, "جستجو محصولات"),
                        new PermissionDto(ShopPermissions.CreateProduct, "ایجاد محصول"),
                        new PermissionDto(ShopPermissions.EditProduct, "ویرایش محصول"),
                    }
                },
                {
                    "productCategories", new List<PermissionDto>
                    {
                        new PermissionDto(ShopPermissions.SearchProductCategories, "جستجو گروه محصولات"),
                        new PermissionDto(ShopPermissions.ListProductCategories, "لیست گروه محصولات"),
                        new PermissionDto(ShopPermissions.CreateProductCategory, "ایجاد گروه محصول"),
                        new PermissionDto(ShopPermissions.EditProductCategory, "ویرایش گروه محصول"),
                    }
                },
                {
                    "Orders", new List<PermissionDto>
                    {
                        new PermissionDto(ShopPermissions.SearchOrders, "جستجو سفارشات"),
                        new PermissionDto(ShopPermissions.ListOrders, "لیست سفارشات"),
                        new PermissionDto(ShopPermissions.ActionOrders, "اکشن های سفارشات"),
                    }
                }
            };
        }
    }
}