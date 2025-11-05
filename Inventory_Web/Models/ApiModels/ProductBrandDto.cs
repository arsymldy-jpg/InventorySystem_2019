// Models/ApiModels/ProductBrandDto.cs
namespace Inventory_Web.Models.ApiModels
{
    public class ProductBrandDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int BrandId { get; set; }
        public string BrandName { get; set; }
    }

    public class CreateProductBrandDto
    {
        public int ProductId { get; set; }
        public int BrandId { get; set; }
    }
}