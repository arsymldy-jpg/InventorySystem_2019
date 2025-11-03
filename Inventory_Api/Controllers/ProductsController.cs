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
using System.Security.Claims;
using Inventory_Api.Services;

namespace Inventory_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AuthorizeRole(Roles.Admin, Roles.SeniorUser, Roles.SeniorStorekeeper, Roles.Storekeeper, Roles.Viewer)]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditService _auditService;

        public ProductsController(ApplicationDbContext context, AuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        // GET: api/Products - همه می‌توانند مشاهده کنند اما با فیلترهای مختلف
        [HttpGet]
        [AuthorizeRole(Roles.Admin, Roles.SeniorUser, Roles.SeniorStorekeeper, Roles.Storekeeper, Roles.Viewer)]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var currentUserRoleId = int.Parse(User.FindFirst("RoleId")?.Value);
            var currentUserRole = Roles.GetRoleName(currentUserRoleId);

            IQueryable<Product> productsQuery = _context.Products
                .Where(p => p.IsActive);

            // اگر کاربر انباردار است، فقط محصولاتی که در انبارهای زیرمجموعه موجود هستند را نشان بده
            if (currentUserRole == Roles.Storekeeper)
            {
                var accessibleWarehouses = await _context.WarehouseAccesses
                    .Where(wa => wa.UserId == currentUserId && wa.CanView)
                    .Select(wa => wa.WarehouseId)
                    .ToListAsync();

                var productIdsInAccessibleWarehouses = await _context.Inventories
                    .Where(i => accessibleWarehouses.Contains(i.WarehouseId) && i.Quantity > 0)
                    .Select(i => i.ProductId)
                    .Distinct()
                    .ToListAsync();

                productsQuery = productsQuery.Where(p => productIdsInAccessibleWarehouses.Contains(p.Id));
            }

            var products = await productsQuery
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Name2 = p.Name2,
                    MainCode = p.MainCode,
                    Code2 = p.Code2,
                    Code3 = p.Code3,
                    TotalQuantity = p.TotalQuantity,
                    ReorderPoint = p.ReorderPoint,
                    SafetyStock = p.SafetyStock,
                    IsActive = p.IsActive
                })
                .ToListAsync();

            return Ok(products);
        }

        // GET: api/Products/5 - همه نقش‌ها می‌توانند مشاهده کنند
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProduct(int id)
        {
            var product = await _context.Products
                .Where(p => p.Id == id && p.IsActive)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Name2 = p.Name2,
                    MainCode = p.MainCode,
                    Code2 = p.Code2,
                    Code3 = p.Code3,
                    TotalQuantity = p.TotalQuantity,
                    ReorderPoint = p.ReorderPoint,
                    SafetyStock = p.SafetyStock,
                    IsActive = p.IsActive
                })
                .FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

        // POST: api/Products - فقط برای Admin, SeniorUser, SeniorStorekeeper
        [HttpPost]
        [AuthorizeRole(Roles.Admin, Roles.SeniorUser, Roles.SeniorStorekeeper)]
        public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductDto createProductDto)
        {
            // بررسی تکراری نبودن کد اصلی
            if (await _context.Products.AnyAsync(p => p.MainCode == createProductDto.MainCode && p.IsActive))
            {
                return BadRequest(new { Message = "کد اصلی کالا تکراری است" });
            }

            var product = new Product
            {
                Name = createProductDto.Name,
                Name2 = createProductDto.Name2,
                MainCode = createProductDto.MainCode,
                Code2 = createProductDto.Code2,
                Code3 = createProductDto.Code3,
                ReorderPoint = createProductDto.ReorderPoint,
                SafetyStock = createProductDto.SafetyStock,
                TotalQuantity = 0, // شروع با موجودی صفر
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // لاگ کردن ایجاد محصول
            await _auditService.LogActionAsync("Products", product.Id, "CREATE",
                null,
                new { product.Name, product.MainCode, product.ReorderPoint, product.SafetyStock },
                $"ایجاد محصول جدید: {product.Name}");

            var productDto = new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Name2 = product.Name2,
                MainCode = product.MainCode,
                Code2 = product.Code2,
                Code3 = product.Code3,
                TotalQuantity = product.TotalQuantity,
                ReorderPoint = product.ReorderPoint,
                SafetyStock = product.SafetyStock,
                IsActive = product.IsActive
            };

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, productDto);
        }

        // PUT: api/Products/5 - فقط برای Admin, SeniorUser, SeniorStorekeeper
        [HttpPut("{id}")]
        [AuthorizeRole(Roles.Admin, Roles.SeniorUser, Roles.SeniorStorekeeper)]
        public async Task<IActionResult> UpdateProduct(int id, ProductDto productDto)
        {
            if (id != productDto.Id)
            {
                return BadRequest();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null || !product.IsActive)
            {
                return NotFound();
            }

            // بررسی تکراری نبودن کد اصلی (به جز خود محصول)
            if (await _context.Products.AnyAsync(p => p.MainCode == productDto.MainCode && p.Id != id && p.IsActive))
            {
                return BadRequest(new { Message = "کد اصلی کالا تکراری است" });
            }

            // ذخیره مقادیر قدیمی برای لاگ
            var oldValues = new
            {
                product.Name,
                product.Name2,
                product.MainCode,
                product.Code2,
                product.Code3,
                product.ReorderPoint,
                product.SafetyStock
            };

            product.Name = productDto.Name;
            product.Name2 = productDto.Name2;
            product.MainCode = productDto.MainCode;
            product.Code2 = productDto.Code2;
            product.Code3 = productDto.Code3;
            product.ReorderPoint = productDto.ReorderPoint;
            product.SafetyStock = productDto.SafetyStock;
            product.ModifiedDate = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();

                // لاگ کردن تغییرات
                await _auditService.LogActionAsync("Products", product.Id, "UPDATE",
                    oldValues,
                    new { product.Name, product.Name2, product.MainCode, product.Code2, product.Code3, product.ReorderPoint, product.SafetyStock },
                    $"ویرایش محصول: {product.Name}");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Products/5 - فقط برای Admin و SeniorUser
        [HttpDelete("{id}")]
        [AuthorizeRole(Roles.Admin, Roles.SeniorUser)]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            // Soft Delete - فقط غیرفعال می‌کنیم
            product.IsActive = false;
            product.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // لاگ کردن حذف محصول
            await _auditService.LogActionAsync("Products", product.Id, "DELETE",
                null, null,
                $"غیرفعال کردن محصول: {product.Name}");

            return NoContent();
        }

        // GET: api/Products/search/{name} - جستجوی محصولات
        [HttpGet("search/{name}")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> SearchProducts(string name)
        {
            var products = await _context.Products
                .Where(p => p.IsActive &&
                           (p.Name.Contains(name) ||
                            p.Name2.Contains(name) ||
                            p.MainCode.Contains(name)))
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Name2 = p.Name2,
                    MainCode = p.MainCode,
                    Code2 = p.Code2,
                    Code3 = p.Code3,
                    TotalQuantity = p.TotalQuantity,
                    ReorderPoint = p.ReorderPoint,
                    SafetyStock = p.SafetyStock,
                    IsActive = p.IsActive
                })
                .ToListAsync();

            return Ok(products);
        }

        // GET: api/Products/low-stock - محصولات با موجودی کم
        [HttpGet("low-stock")]
        [AuthorizeRole(Roles.Admin, Roles.SeniorUser, Roles.SeniorStorekeeper, Roles.Storekeeper)]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetLowStockProducts()
        {
            var products = await _context.Products
                .Where(p => p.IsActive && p.TotalQuantity <= p.ReorderPoint)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Name2 = p.Name2,
                    MainCode = p.MainCode,
                    Code2 = p.Code2,
                    Code3 = p.Code3,
                    TotalQuantity = p.TotalQuantity,
                    ReorderPoint = p.ReorderPoint,
                    SafetyStock = p.SafetyStock,
                    IsActive = p.IsActive
                })
                .ToListAsync();

            return Ok(products);
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id && e.IsActive);
        }
    }
}