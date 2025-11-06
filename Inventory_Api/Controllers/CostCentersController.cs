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
    public class CostCentersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CostCentersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/CostCenters
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CostCenterDto>>> GetCostCenters()
        {
            var costCenters = await _context.CostCenters
                .Where(cc => cc.IsActive)
                .Select(cc => new CostCenterDto
                {
                    Id = cc.Id,
                    Name = cc.Name,
                    Description = cc.Description,
                    IsActive = cc.IsActive,
                    CreatedDate = cc.CreatedDate,
                    CreatedBy = cc.CreatedBy
                })
                .ToListAsync();

            return Ok(costCenters);
        }

        // GET: api/CostCenters/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CostCenterDto>> GetCostCenter(int id)
        {
            try
            {
                var costCenter = await _context.CostCenters
                    .Where(cc => cc.Id == id && cc.IsActive)
                    .Select(cc => new CostCenterDto
                    {
                        Id = cc.Id,
                        Name = cc.Name,
                        Description = cc.Description,
                        IsActive = cc.IsActive,
                        CreatedDate = cc.CreatedDate,
                        CreatedBy = cc.CreatedBy
                    })
                    .FirstOrDefaultAsync();

                if (costCenter == null)
                {
                    return NotFound();
                }

                return costCenter;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ خطا در دریافت مرکز هزینه {id}: {ex.Message}");
                return StatusCode(500, "خطا در سرور");
            }
        }

        // POST: api/CostCenters
        [HttpPost]
        [AuthorizeRole(Roles.Admin, Roles.SeniorUser, Roles.SeniorStorekeeper)]
        public async Task<ActionResult<CostCenterDto>> CreateCostCenter(CreateCostCenterDto createCostCenterDto)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var costCenter = new CostCenter
            {
                Name = createCostCenterDto.Name,
                Description = createCostCenterDto.Description,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = userId
            };

            _context.CostCenters.Add(costCenter);
            await _context.SaveChangesAsync();

            var costCenterDto = new CostCenterDto
            {
                Id = costCenter.Id,
                Name = costCenter.Name,
                Description = costCenter.Description,
                IsActive = costCenter.IsActive,
                CreatedDate = costCenter.CreatedDate,
                CreatedBy = costCenter.CreatedBy
            };

            return CreatedAtAction(nameof(GetCostCenters), new { id = costCenter.Id }, costCenterDto);
        }

        // PUT: api/CostCenters/5
        [HttpPut("{id}")]
        [AuthorizeRole(Roles.Admin, Roles.SeniorUser, Roles.SeniorStorekeeper)]
        public async Task<IActionResult> UpdateCostCenter(int id, CostCenterDto costCenterDto)
        {
            if (id != costCenterDto.Id)
            {
                return BadRequest();
            }

            var costCenter = await _context.CostCenters.FindAsync(id);
            if (costCenter == null || !costCenter.IsActive)
            {
                return NotFound();
            }

            costCenter.Name = costCenterDto.Name;
            costCenter.Description = costCenterDto.Description;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/CostCenters/5
        [HttpDelete("{id}")]
        [AuthorizeRole(Roles.Admin, Roles.SeniorUser, Roles.SeniorStorekeeper)]
        public async Task<IActionResult> DeleteCostCenter(int id)
        {
            var costCenter = await _context.CostCenters.FindAsync(id);
            if (costCenter == null)
            {
                return NotFound();
            }

            costCenter.IsActive = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // به فایل CostCentersController.cs در پروژه API اضافه شود

        // GET: api/CostCenters/{id}/usage - بررسی استفاده از مرکز هزینه در عملیات انبار
        [HttpGet("{id}/usage")]
        public async Task<ActionResult<bool>> CheckCostCenterUsage(int id)
        {
            try
            {
                var isUsed = await _context.StockOperations
                    .AnyAsync(so => so.CostCenterId == id);

                return Ok(isUsed);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ خطا در بررسی استفاده مرکز هزینه: {ex.Message}");
                return StatusCode(500, "خطا در بررسی استفاده مرکز هزینه");
            }
        }
    }
}