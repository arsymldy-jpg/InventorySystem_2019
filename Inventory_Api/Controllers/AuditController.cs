using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Inventory_Api.Services;
using Inventory_Api.Models.DTOs;
using Inventory_Api.Helpers;

namespace Inventory_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AuthorizeRole(Roles.Admin, Roles.SeniorUser)]
    public class AuditController : ControllerBase
    {
        private readonly AuditService _auditService;

        public AuditController(AuditService auditService)
        {
            _auditService = auditService;
        }

        [HttpGet]
        public async Task<ActionResult> GetAuditLogs(
            [FromQuery] string tableName = null,
            [FromQuery] int? recordId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var logs = await _auditService.GetAuditLogsAsync(tableName, recordId, fromDate, toDate);
            return Ok(logs);
        }

        [HttpGet("users/{userId}")]
        public async Task<ActionResult> GetUserAuditLogs(int userId, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            var logs = await _auditService.GetAuditLogsAsync("Users", userId, fromDate, toDate);
            return Ok(logs);
        }

        [HttpGet("products/{productId}")]
        public async Task<ActionResult> GetProductAuditLogs(int productId, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            var logs = await _auditService.GetAuditLogsAsync("Products", productId, fromDate, toDate);
            return Ok(logs);
        }

        [HttpGet("warehouses/{warehouseId}")]
        public async Task<ActionResult> GetWarehouseAuditLogs(int warehouseId, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            var logs = await _auditService.GetAuditLogsAsync("Warehouses", warehouseId, fromDate, toDate);
            return Ok(logs);
        }
    }
}