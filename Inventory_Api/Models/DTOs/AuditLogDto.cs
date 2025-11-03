using System;

namespace Inventory_Api.Models.DTOs
{
    public class AuditLogDto
    {
        public int Id { get; set; }
        public string TableName { get; set; }
        public int RecordId { get; set; }
        public string Action { get; set; }
        public string OldValues { get; set; }
        public string NewValues { get; set; }
        public string Description { get; set; }
        public string UserName { get; set; }
        public int UserId { get; set; }
        public DateTime Timestamp { get; set; }
        public string IpAddress { get; set; }
    }
}