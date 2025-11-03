namespace Inventory_Api.Models.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Name2 { get; set; }
        public string MainCode { get; set; }
        public string Code2 { get; set; }
        public string Code3 { get; set; }
        public int TotalQuantity { get; set; }
        public int ReorderPoint { get; set; }
        public int SafetyStock { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateProductDto
    {
        public string Name { get; set; }
        public string Name2 { get; set; }
        public string MainCode { get; set; }
        public string Code2 { get; set; }
        public string Code3 { get; set; }
        public int ReorderPoint { get; set; }
        public int SafetyStock { get; set; }
    }
}