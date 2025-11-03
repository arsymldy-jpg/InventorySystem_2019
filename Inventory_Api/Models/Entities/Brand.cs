using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Inventory_Api.Models.Entities
{
    public class Brand
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual ICollection<ProductBrand> ProductBrands { get; set; }
    }
}