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
using Inventory_Api.Services;

namespace Inventory_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AuthorizeRole(Roles.Admin, Roles.SeniorUser, Roles.SeniorStorekeeper)]
    public class BrandsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditService _auditService;

        public BrandsController(ApplicationDbContext context, AuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        // GET: api/Brands - همه می‌توانند مشاهده کنند
        [HttpGet]
        [AuthorizeRole(Roles.Admin, Roles.SeniorUser, Roles.SeniorStorekeeper, Roles.Storekeeper, Roles.Viewer)]
        public async Task<ActionResult<IEnumerable<BrandDto>>> GetBrands()
        {
            var brands = await _context.Brands
                .Where(b => b.IsActive)
                .Select(b => new BrandDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    Description = b.Description,
                    IsActive = b.IsActive
                })
                .ToListAsync();

            return Ok(brands);
        }

        // GET: api/Brands/5 - دریافت یک برند خاص
        [HttpGet("{id}")]
        //[AuthorizeRole(Roles.Admin, Roles.SeniorUser, Roles.SeniorStorekeeper, Roles.Storekeeper, Roles.Viewer)]
        public async Task<ActionResult<BrandDto>> GetBrand(int id)
        {
            var brand = await _context.Brands
                .Where(b => b.Id == id && b.IsActive)
                .Select(b => new BrandDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    Description = b.Description,
                    IsActive = b.IsActive
                })
                .FirstOrDefaultAsync();

            if (brand == null)
            {
                return NotFound();
            }

            return Ok(brand);
        }


        // POST: api/Brands
        [HttpPost]
        public async Task<ActionResult<BrandDto>> CreateBrand(CreateBrandDto createBrandDto)
        {
            var brand = new Brand
            {
                Name = createBrandDto.Name,
                Description = createBrandDto.Description,
                IsActive = true
            };

            _context.Brands.Add(brand);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync("Brands", brand.Id, "CREATE",
                null,
                new { brand.Name, brand.Description },
                $"ایجاد برند جدید: {brand.Name}");

            var brandDto = new BrandDto
            {
                Id = brand.Id,
                Name = brand.Name,
                Description = brand.Description,
                IsActive = brand.IsActive
            };

            return CreatedAtAction(nameof(GetBrands), new { id = brand.Id }, brandDto);
        }

        // PUT: api/Brands/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBrand(int id, BrandDto brandDto)
        {
            if (id != brandDto.Id)
            {
                return BadRequest();
            }

            var brand = await _context.Brands.FindAsync(id);
            if (brand == null || !brand.IsActive)
            {
                return NotFound();
            }

            var oldValues = new { brand.Name, brand.Description };

            brand.Name = brandDto.Name;
            brand.Description = brandDto.Description;

            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync("Brands", brand.Id, "UPDATE",
                oldValues,
                new { brand.Name, brand.Description },
                $"ویرایش برند: {brand.Name}");

            return NoContent();
        }

        // DELETE: api/Brands/5
        [HttpDelete("{id}")]
        [AuthorizeRole(Roles.Admin, Roles.SeniorUser)]
        public async Task<IActionResult> DeleteBrand(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null)
            {
                return NotFound();
            }

            brand.IsActive = false;
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync("Brands", brand.Id, "DELETE",
                null, null,
                $"غیرفعال کردن برند: {brand.Name}");

            return NoContent();
        }
    }
}