namespace Inventory_Api.Models.DTOs
{
    public class WarehouseAccessDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public bool CanEdit { get; set; }
        public bool CanView { get; set; }
    }

    public class CreateWarehouseAccessDto
    {
        public int UserId { get; set; }
        public int WarehouseId { get; set; }
        public bool CanEdit { get; set; }
        public bool CanView { get; set; } = true;
    }

    public class UpdateWarehouseAccessDto
    {
        public bool CanEdit { get; set; }
        public bool CanView { get; set; }
    }
}