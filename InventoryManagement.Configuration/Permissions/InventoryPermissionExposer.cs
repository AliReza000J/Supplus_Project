using _0_Framework.Infrastructure;

namespace InventoryManagement.Infrastructure.Configuration.Permissions
{
    public class InventoryPermissionExposer : IPermissionExposer
    {
        public Dictionary<string, List<PermissionDto>> Expose()
        {
            return new Dictionary<string, List<PermissionDto>>
            {
                {
                    "inventory", new List<PermissionDto>
                    {
                        new PermissionDto(InventoryPermissions.ListInventory, "لیست انبار"),
                        new PermissionDto(InventoryPermissions.SearchInventory, "جستجوی انبار"),
                        new PermissionDto(InventoryPermissions.CreateInventory, "ساخت انبار"),
                        new PermissionDto(InventoryPermissions.EditInventory, "ویرایش انبار"),
                        new PermissionDto(InventoryPermissions.Increase, "افزایش موجودی"),
                        new PermissionDto(InventoryPermissions.Reduce, "کاهش موجودی"),
                        new PermissionDto(InventoryPermissions.OperationLog, "مشاهده گردش انبار")
                    }
                }
            };
        }
    }
}