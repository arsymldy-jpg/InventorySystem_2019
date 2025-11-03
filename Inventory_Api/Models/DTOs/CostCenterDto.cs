using System;

namespace Inventory_Api.Models.DTOs
{
    public class CostCenterDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
    }

    public class CreateCostCenterDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}