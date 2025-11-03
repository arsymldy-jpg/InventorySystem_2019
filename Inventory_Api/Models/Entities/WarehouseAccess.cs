using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory_Api.Models.Entities
{
    public class WarehouseAccess
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int WarehouseId { get; set; }

        [Required]
        public bool CanEdit { get; set; } // دسترسی ویرایش

        [Required]
        public bool CanView { get; set; } = true; // دسترسی مشاهده

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("WarehouseId")]
        public virtual Warehouse Warehouse { get; set; }
    }
}