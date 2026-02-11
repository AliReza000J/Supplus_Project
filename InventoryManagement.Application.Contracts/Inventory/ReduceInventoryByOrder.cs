namespace InventoryManagement.Application.Contracts.Inventory
{
    public class ReduceInventoryByOrder
    {
        public long InventoryId { get; set; }
        public long ProductId { get; set; }
        public long UserId { get; set; }
        public long Count { get; set; }
        public string Description { get; set; } = "بدون توضیحات";
        public long OrderId { get; set; } = 0;

        public ReduceInventoryByOrder()
        {

        }

    }
}
