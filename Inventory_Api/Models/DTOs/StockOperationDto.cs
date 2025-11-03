using System;

namespace Inventory_Api.Models.DTOs
{
    public class StockOperationDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public int BrandId { get; set; }
        public string BrandName { get; set; }
        public int Quantity { get; set; }
        public string OperationType { get; set; }
        public int? CostCenterId { get; set; }
        public string CostCenterName { get; set; }
        public string Reason { get; set; }
        public DateTime OperationDate { get; set; }
        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; }
    }

    public class IssueStockDto
    {
        public int ProductId { get; set; }
        public int WarehouseId { get; set; }
        public int BrandId { get; set; }
        public int Quantity { get; set; }
        public int CostCenterId { get; set; }
        public string Reason { get; set; }
    }

    public class ReceiveStockDto
    {
        public int ProductId { get; set; }
        public int WarehouseId { get; set; }
        public int BrandId { get; set; }
        public int Quantity { get; set; }
        public string Reason { get; set; }
    }
}