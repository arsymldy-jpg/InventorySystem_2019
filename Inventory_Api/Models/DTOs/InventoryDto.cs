using System;

namespace Inventory_Api.Models.DTOs
{
    public class InventoryDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public int BrandId { get; set; }
        public string BrandName { get; set; }
        public int Quantity { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class AdjustInventoryDto
    {
        public int ProductId { get; set; }
        public int WarehouseId { get; set; }
        public int BrandId { get; set; }
        public int NewQuantity { get; set; }
        public string Reason { get; set; }
    }
}