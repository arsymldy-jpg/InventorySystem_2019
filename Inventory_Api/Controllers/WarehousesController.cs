using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Inventory_Api.Data;
using Inventory_Api.Models.Entities;
using Inventory_Api.Models.DTOs;
using Inventory_Api.Helpers;

namespace Inventory_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AuthorizeRole(Roles.Admin, Roles.SeniorUser, Roles.SeniorStorekeeper, Roles.Storekeeper, Roles.Viewer)]
    public class WarehousesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public WarehousesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Warehouses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WarehouseDto>>> GetWarehouses()
        {
            var warehouses = await _context.Warehouses
                .Where(w => w.IsActive)
                .Select(w => new WarehouseDto
                {
                    Id = w.Id,
                    Name = w.Name,
                    Address = w.Address,
                    Phone = w.Phone,
                    IsActive = w.IsActive,
                    CreatedDate = w.CreatedDate
                })
                .ToListAsync();

            return Ok(warehouses);
        }

        // GET: api/Warehouses/5
        [HttpGet("{id}")]
        public async Task<ActionResult<WarehouseDto>> GetWarehouse(int id)
        {
            var warehouse = await _context.Warehouses
                .Where(w => w.Id == id && w.IsActive)
                .Select(w => new WarehouseDto
                {
                    Id = w.Id,
                    Name = w.Name,
                    Address = w.Address,
                    Phone = w.Phone,
                    IsActive = w.IsActive,
                    CreatedDate = w.CreatedDate
                })
                .FirstOrDefaultAsync();

            if (warehouse == null)
            {
                return NotFound();
            }

            return warehouse;
        }

        // POST: api/Warehouses - فقط برای Admin و SeniorUser
        [HttpPost]
        [AuthorizeRole(Roles.Admin, Roles.SeniorUser)]
        public async Task<ActionResult<WarehouseDto>> CreateWarehouse(CreateWarehouseDto createWarehouseDto)
        {
            var warehouse = new Warehouse
            {
                Name = createWarehouseDto.Name,
                Address = createWarehouseDto.Address,
                Phone = createWarehouseDto.Phone,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            _context.Warehouses.Add(warehouse);
            await _context.SaveChangesAsync();

            var warehouseDto = new WarehouseDto
            {
                Id = warehouse.Id,
                Name = warehouse.Name,
                Address = warehouse.Address,
                Phone = warehouse.Phone,
                IsActive = warehouse.IsActive,
                CreatedDate = warehouse.CreatedDate
            };

            return CreatedAtAction(nameof(GetWarehouse), new { id = warehouse.Id }, warehouseDto);
        }

        // PUT: api/Warehouses/5 - فقط برای Admin و SeniorUser
        [HttpPut("{id}")]
        [AuthorizeRole(Roles.Admin, Roles.SeniorUser)]
        public async Task<IActionResult> UpdateWarehouse(int id, WarehouseDto warehouseDto)
        {
            if (id != warehouseDto.Id)
            {
                return BadRequest();
            }

            var warehouse = await _context.Warehouses.FindAsync(id);
            if (warehouse == null || !warehouse.IsActive)
            {
                return NotFound();
            }

            warehouse.Name = warehouseDto.Name;
            warehouse.Address = warehouseDto.Address;
            warehouse.Phone = warehouseDto.Phone;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Warehouses/5 - فقط برای Admin
        [HttpDelete("{id}")]
        [AuthorizeRole(Roles.Admin)]
        public async Task<IActionResult> DeleteWarehouse(int id)
        {
            var warehouse = await _context.Warehouses.FindAsync(id);
            if (warehouse == null)
            {
                return NotFound();
            }

            // Soft Delete
            warehouse.IsActive = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}