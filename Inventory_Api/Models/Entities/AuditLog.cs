using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory_Api.Models.Entities
{
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string TableName { get; set; }

        [Required]
        public int RecordId { get; set; }

        [Required]
        public string Action { get; set; } // CREATE, UPDATE, DELETE

        [Column(TypeName = "text")]
        public string OldValues { get; set; }

        [Column(TypeName = "text")]
        public string NewValues { get; set; }

        [Column(TypeName = "text")]
        public string Description { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [MaxLength(45)]
        public string IpAddress { get; set; }

        // Navigation Properties
        public virtual User User { get; set; }
    }
}