namespace InventoryManagement.Application.Contracts.Inventory
{
    public class ReduceInventory
    {
        public long InventoryId { get; set; }
        public long ProductId { get; set; }
        public long Count { get; set; }
        public string Description { get; set; } = "بدون توضیحات";
        public long OrderId { get; set; } = 0;

        public ReduceInventory()
        {
            
        }

        //public ReduceInventory(long productId, long count, string description, long orderId)
        //{
        //    ProductId = productId;
        //    Count = count;
        //    Description = description;
        //    OrderId = orderId;
        //}
    }
}
