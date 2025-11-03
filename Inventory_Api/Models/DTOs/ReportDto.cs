using System;
using System.Collections.Generic;

namespace Inventory_Api.Models.DTOs
{
    public class InventorySummaryReport
    {
        public int TotalProducts { get; set; }
        public int TotalWarehouses { get; set; }
        public int TotalUsers { get; set; }
        public int LowStockProducts { get; set; }
        public decimal TotalInventoryValue { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class WarehouseInventoryReport
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public int TotalProducts { get; set; }
        public int TotalQuantity { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class BrandSummaryReport
    {
        public int BrandId { get; set; }
        public string BrandName { get; set; }
        public int TotalProducts { get; set; }
        public int TotalQuantity { get; set; }
        public int WarehousesCount { get; set; }
    }

    public class LowStockAlertReport
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string MainCode { get; set; }
        public int CurrentQuantity { get; set; }
        public int ReorderPoint { get; set; }
        public int SafetyStock { get; set; }
        public int ShortageAmount { get; set; }
        public string AlertLevel { get; set; }
    }

    public class UserActivityReport
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string RoleName { get; set; }
        public DateTime? LastLogin { get; set; }
        public int LoginCount { get; set; }
        public int ActionsCount { get; set; }
    }

    public class BrandInventoryReport
    {
        public int BrandId { get; set; }
        public string BrandName { get; set; }
        public int TotalProducts { get; set; }
        public int TotalQuantity { get; set; }
        public List<WarehouseQuantity> WarehouseDistribution { get; set; }
    }

    public class ProductBrandSummaryReport
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string MainCode { get; set; }
        public int TotalQuantity { get; set; }
        public List<BrandQuantity> Brands { get; set; }
    }

    public class WarehouseQuantity
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public int Quantity { get; set; }
    }

    public class BrandQuantity
    {
        public int BrandId { get; set; }
        public string BrandName { get; set; }
        public int TotalQuantity { get; set; }
    }
}