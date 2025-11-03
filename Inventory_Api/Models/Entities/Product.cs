using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory_Api.Models.Entities
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(200)]
        public string Name2 { get; set; }

        [Required]
        [MaxLength(100)]
        public string MainCode { get; set; }

        [MaxLength(100)]
        public string Code2 { get; set; }

        [MaxLength(100)]
        public string Code3 { get; set; }

        public int TotalQuantity { get; set; }
        public int ReorderPoint { get; set; }
        public int SafetyStock { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedDate { get; set; }

        // Navigation Properties
        public virtual ICollection<ProductBrand> ProductBrands { get; set; }
        public virtual ICollection<Inventory> Inventories { get; set; }
    }
}