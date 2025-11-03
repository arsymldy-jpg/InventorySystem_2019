using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Inventory_Api.Models.Entities
{
    public class Role
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } // Admin, SeniorUser, SeniorStorekeeper, Storekeeper, Viewer

        [MaxLength(500)]
        public string Description { get; set; }

        public virtual ICollection<User> Users { get; set; }
    }
}