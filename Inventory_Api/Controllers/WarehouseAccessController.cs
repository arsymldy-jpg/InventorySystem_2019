using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Inventory_Api.Data;
using Inventory_Api.Models.Entities;
using Inventory_Api.Models.DTOs;
using Inventory_Api.Helpers;
using Inventory_Api.Services;

namespace Inventory_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AuthorizeRole(Roles.Admin, Roles.SeniorUser, Roles.SeniorStorekeeper, Roles.Storekeeper,Roles.Viewer)]
    public class WarehouseAccessController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditService _auditService;

        public WarehouseAccessController(ApplicationDbContext context, AuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        // GET: api/WarehouseAccess/user/5
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<WarehouseAccessDto>>> GetUserWarehouseAccess(int userId)
        {
            var accessList = await _context.WarehouseAccesses
                .Include(wa => wa.User)
                .Include(wa => wa.Warehouse)
                .Where(wa => wa.UserId == userId)
                .Select(wa => new WarehouseAccessDto
                {
                    Id = wa.Id,
                    UserId = wa.UserId,
                    UserName = $"{wa.User.FirstName} {wa.User.LastName}",
                    WarehouseId = wa.WarehouseId,
                    WarehouseName = wa.Warehouse.Name,
                    CanEdit = wa.CanEdit,
                    CanView = wa.CanView
                })
                .ToListAsync();

            return Ok(accessList);
        }

        // POST: api/WarehouseAccess
        [HttpPost]
        [AuthorizeRole(Roles.Admin, Roles.SeniorUser, Roles.SeniorStorekeeper)]
        public async Task<ActionResult<WarehouseAccessDto>> CreateWarehouseAccess(CreateWarehouseAccessDto createDto)
        {
            // بررسی وجود دسترسی تکراری
            var existing = await _context.WarehouseAccesses
                .FirstOrDefaultAsync(wa => wa.UserId == createDto.UserId && wa.WarehouseId == createDto.WarehouseId);

            if (existing != null)
            {
                return BadRequest("دسترسی به این انبار قبلاً برای کاربر تعریف شده است");
            }

            var warehouseAccess = new WarehouseAccess
            {
                UserId = createDto.UserId,
                WarehouseId = createDto.WarehouseId,
                CanEdit = createDto.CanEdit,
                CanView = createDto.CanView
            };

            _context.WarehouseAccesses.Add(warehouseAccess);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync("WarehouseAccess", warehouseAccess.Id, "CREATE",
                null,
                new { warehouseAccess.UserId, warehouseAccess.WarehouseId, warehouseAccess.CanEdit, warehouseAccess.CanView },
                $"تعریف دسترسی انبار برای کاربر");

            var warehouseAccessDto = await _context.WarehouseAccesses
                .Include(wa => wa.User)
                .Include(wa => wa.Warehouse)
                .Where(wa => wa.Id == warehouseAccess.Id)
                .Select(wa => new WarehouseAccessDto
                {
                    Id = wa.Id,
                    UserId = wa.UserId,
                    UserName = $"{wa.User.FirstName} {wa.User.LastName}",
                    WarehouseId = wa.WarehouseId,
                    WarehouseName = wa.Warehouse.Name,
                    CanEdit = wa.CanEdit,
                    CanView = wa.CanView
                })
                .FirstOrDefaultAsync();

            return Ok(warehouseAccessDto);
        }

        // PUT: api/WarehouseAccess/5
        [HttpPut("{id}")]
        [AuthorizeRole(Roles.Admin, Roles.SeniorUser, Roles.SeniorStorekeeper)]
        public async Task<IActionResult> UpdateWarehouseAccess(int id, UpdateWarehouseAccessDto updateDto)
        {
            var warehouseAccess = await _context.WarehouseAccesses.FindAsync(id);
            if (warehouseAccess == null)
            {
                return NotFound();
            }

            var oldValues = new { warehouseAccess.CanEdit, warehouseAccess.CanView };

            warehouseAccess.CanEdit = updateDto.CanEdit;
            warehouseAccess.CanView = updateDto.CanView;

            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync("WarehouseAccess", warehouseAccess.Id, "UPDATE",
                oldValues,
                new { warehouseAccess.CanEdit, warehouseAccess.CanView },
                $"ویرایش دسترسی انبار");

            return NoContent();
        }

        // DELETE: api/WarehouseAccess/5
        [HttpDelete("{id}")]
        [AuthorizeRole(Roles.Admin, Roles.SeniorUser, Roles.SeniorStorekeeper)]
        public async Task<IActionResult> DeleteWarehouseAccess(int id)
        {
            var warehouseAccess = await _context.WarehouseAccesses.FindAsync(id);
            if (warehouseAccess == null)
            {
                return NotFound();
            }

            _context.WarehouseAccesses.Remove(warehouseAccess);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync("WarehouseAccess", warehouseAccess.Id, "DELETE",
                null, null,
                $"حذف دسترسی انبار");

            return NoContent();
        }

        // GET: api/WarehouseAccess/my-access
        [HttpGet("my-access")]
        [AuthorizeRole(Roles.Storekeeper)]
        public async Task<ActionResult<IEnumerable<WarehouseAccessDto>>> GetMyWarehouseAccess()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var accessList = await _context.WarehouseAccesses
                .Include(wa => wa.Warehouse)
                .Where(wa => wa.UserId == userId)
                .Select(wa => new WarehouseAccessDto
                {
                    Id = wa.Id,
                    UserId = wa.UserId,
                    WarehouseId = wa.WarehouseId,
                    WarehouseName = wa.Warehouse.Name,
                    CanEdit = wa.CanEdit,
                    CanView = wa.CanView
                })
                .ToListAsync();

            return Ok(accessList);
        }
    }
}