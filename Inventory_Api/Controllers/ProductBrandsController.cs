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
    [AuthorizeRole(Roles.Admin, Roles.SeniorUser, Roles.SeniorStorekeeper)]
    public class ProductBrandsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductBrandsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/ProductBrands
        [HttpPost]
        public async Task<ActionResult<ProductBrandDto>> AddBrandToProduct(CreateProductBrandDto createDto)
        {
            // بررسی وجود ارتباط
            var existing = await _context.ProductBrands
                .FirstOrDefaultAsync(pb => pb.ProductId == createDto.ProductId && pb.BrandId == createDto.BrandId);

            if (existing != null)
            {
                return BadRequest("این برند قبلاً به محصول اضافه شده است");
            }

            var productBrand = new ProductBrand
            {
                ProductId = createDto.ProductId,
                BrandId = createDto.BrandId
            };

            _context.ProductBrands.Add(productBrand);
            await _context.SaveChangesAsync();

            var productBrandDto = await _context.ProductBrands
                .Include(pb => pb.Product)
                .Include(pb => pb.Brand)
                .Where(pb => pb.Id == productBrand.Id)
                .Select(pb => new ProductBrandDto
                {
                    Id = pb.Id,
                    ProductId = pb.ProductId,
                    ProductName = pb.Product.Name,
                    BrandId = pb.BrandId,
                    BrandName = pb.Brand.Name
                })
                .FirstOrDefaultAsync();

            return Ok(productBrandDto);
        }

        // GET: api/ProductBrands/product/5
        [HttpGet("product/{productId}")]
        [AuthorizeRole(Roles.Admin, Roles.SeniorUser, Roles.SeniorStorekeeper, Roles.Storekeeper, Roles.Viewer)]
        public async Task<ActionResult<IEnumerable<ProductBrandDto>>> GetProductBrands(int productId)
        {
            var productBrands = await _context.ProductBrands
                .Include(pb => pb.Product)
                .Include(pb => pb.Brand)
                .Where(pb => pb.ProductId == productId)
                .Select(pb => new ProductBrandDto
                {
                    Id = pb.Id,
                    ProductId = pb.ProductId,
                    ProductName = pb.Product.Name,
                    BrandId = pb.BrandId,
                    BrandName = pb.Brand.Name
                })
                .ToListAsync();

            return Ok(productBrands);
        }

        // DELETE: api/ProductBrands/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveBrandFromProduct(int id)
        {
            var productBrand = await _context.ProductBrands.FindAsync(id);
            if (productBrand == null)
            {
                return NotFound();
            }

            _context.ProductBrands.Remove(productBrand);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}