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

namespace Inventory_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AuthorizeRole(Roles.Admin, Roles.SeniorUser, Roles.SeniorStorekeeper, Roles.Storekeeper)]
    public class InventoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public InventoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Inventory - مشاهده موجودی همه انبارها
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryDto>>> GetInventory()
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var currentUserRoleId = int.Parse(User.FindFirst("RoleId")?.Value);
            var currentUserRole = Roles.GetRoleName(currentUserRoleId);

            IQueryable<Inventory> inventoryQuery = _context.Inventories
                .Include(i => i.Product)
                .Include(i => i.Warehouse)
                .Include(i => i.Brand);

            // فیلتر کردن بر اساس دسترسی کاربر
            if (currentUserRole == Roles.Storekeeper)
            {
                // انباردار فقط می‌تواند انبارهای زیرمجموعه خود را ببیند
                var accessibleWarehouses = await _context.WarehouseAccesses
                    .Where(wa => wa.UserId == currentUserId && wa.CanView)
                    .Select(wa => wa.WarehouseId)
                    .ToListAsync();

                inventoryQuery = inventoryQuery.Where(i => accessibleWarehouses.Contains(i.WarehouseId));
            }

            var inventory = await inventoryQuery
                .Select(i => new InventoryDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product.Name,
                    WarehouseId = i.WarehouseId,
                    WarehouseName = i.Warehouse.Name,
                    BrandId = i.BrandId,
                    BrandName = i.Brand.Name,
                    Quantity = i.Quantity,
                    LastUpdated = i.LastUpdated
                })
                .ToListAsync();

            return Ok(inventory);
        }

        // POST: api/Inventory/adjust - تنظیم موجودی
        [HttpPost("adjust")]
        public async Task<ActionResult<InventoryDto>> AdjustInventory(AdjustInventoryDto adjustDto)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var currentUserRoleId = int.Parse(User.FindFirst("RoleId")?.Value);
            var currentUserRole = Roles.GetRoleName(currentUserRoleId);

            // بررسی دسترسی به انبار
            if (currentUserRole == Roles.Storekeeper)
            {
                var canEdit = await _context.WarehouseAccesses
                    .AnyAsync(wa => wa.UserId == currentUserId &&
                                   wa.WarehouseId == adjustDto.WarehouseId &&
                                   wa.CanEdit);

                if (!canEdit)
                {
                    return Forbid("شما مجوز ویرایش این انبار را ندارید");
                }
            }

            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.ProductId == adjustDto.ProductId &&
                                         i.WarehouseId == adjustDto.WarehouseId &&
                                         i.BrandId == adjustDto.BrandId);

            if (inventory == null)
            {
                // ایجاد رکورد جدید
                inventory = new Inventory
                {
                    ProductId = adjustDto.ProductId,
                    WarehouseId = adjustDto.WarehouseId,
                    BrandId = adjustDto.BrandId,
                    Quantity = adjustDto.NewQuantity,
                    LastUpdated = DateTime.UtcNow
                };
                _context.Inventories.Add(inventory);
            }
            else
            {
                // به روزرسانی موجودی
                inventory.Quantity = adjustDto.NewQuantity;
                inventory.LastUpdated = DateTime.UtcNow;
            }

            // به روزرسانی موجودی کل محصول
            await UpdateProductTotalQuantity(adjustDto.ProductId);

            await _context.SaveChangesAsync();

            var inventoryDto = await _context.Inventories
                .Include(i => i.Product)
                .Include(i => i.Warehouse)
                .Include(i => i.Brand)
                .Where(i => i.Id == inventory.Id)
                .Select(i => new InventoryDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product.Name,
                    WarehouseId = i.WarehouseId,
                    WarehouseName = i.Warehouse.Name,
                    BrandId = i.BrandId,
                    BrandName = i.Brand.Name,
                    Quantity = i.Quantity,
                    LastUpdated = i.LastUpdated
                })
                .FirstOrDefaultAsync();

            return Ok(inventoryDto);
        }

        // GET: api/Inventory/warehouse/5 - موجودی یک انبار خاص
        [HttpGet("warehouse/{warehouseId}")]
        public async Task<ActionResult<IEnumerable<InventoryDto>>> GetWarehouseInventory(int warehouseId)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var currentUserRoleId = int.Parse(User.FindFirst("RoleId")?.Value);
            var currentUserRole = Roles.GetRoleName(currentUserRoleId);

            // بررسی دسترسی مشاهده
            if (currentUserRole == Roles.Storekeeper)
            {
                var canView = await _context.WarehouseAccesses
                    .AnyAsync(wa => wa.UserId == currentUserId &&
                                   wa.WarehouseId == warehouseId &&
                                   wa.CanView);

                if (!canView)
                {
                    return Forbid("شما مجوز مشاهده این انبار را ندارید");
                }
            }

            var inventory = await _context.Inventories
                .Include(i => i.Product)
                .Include(i => i.Warehouse)
                .Include(i => i.Brand)
                .Where(i => i.WarehouseId == warehouseId)
                .Select(i => new InventoryDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product.Name,
                    WarehouseId = i.WarehouseId,
                    WarehouseName = i.Warehouse.Name,
                    BrandId = i.BrandId,
                    BrandName = i.Brand.Name,
                    Quantity = i.Quantity,
                    LastUpdated = i.LastUpdated
                })
                .ToListAsync();

            return Ok(inventory);
        }

        private async Task UpdateProductTotalQuantity(int productId)
        {
            var totalQuantity = await _context.Inventories
                .Where(i => i.ProductId == productId)
                .SumAsync(i => i.Quantity);

            var product = await _context.Products.FindAsync(productId);
            if (product != null)
            {
                product.TotalQuantity = totalQuantity;
                product.ModifiedDate = DateTime.UtcNow;
            }
        }

        // GET: api/Inventory/warehouses-with-stock/{productId} - انبارهای دارای موجودی یک کالا
        [HttpGet("warehouses-with-stock/{productId}")]
        public async Task<ActionResult<IEnumerable<WarehouseStockInfoDto>>> GetWarehousesWithStock(int productId)
        {
            
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var currentUserRoleId = int.Parse(User.FindFirst("RoleId")?.Value);
                var currentUserRole = Roles.GetRoleName(currentUserRoleId);
            //Console.WriteLine(currentUserId);
            //return StatusCode(511, currentUserRole);

            try
            {
                // دریافت انبارهای قابل ویرایش برای کاربر
                IQueryable<int> editableWarehouseIds;

                if (currentUserRole == Roles.Storekeeper)
                {
                    editableWarehouseIds = _context.WarehouseAccesses
                        .Where(wa => wa.UserId == currentUserId && wa.CanEdit)
                        .Select(wa => wa.WarehouseId);
                }
                else
                {
                    // سایر نقش‌ها به همه انبارها دسترسی دارند
                    editableWarehouseIds = _context.Warehouses
                        .Where(w => w.IsActive)
                        .Select(w => w.Id);
                }

                // انبارهایی که کاربر دسترسی دارد و کالا در آنها موجود است
                var warehousesWithStock = await _context.Inventories
                    .Include(i => i.Warehouse)
                    .Where(i => i.ProductId == productId &&
                               i.Quantity > 0 &&
                               editableWarehouseIds.Contains(i.WarehouseId))
                    .Select(i => new WarehouseStockInfoDto
                    {
                        WarehouseId = i.WarehouseId,
                        WarehouseName = i.Warehouse.Name,
                        Quantity = i.Quantity
                    })
                    .Distinct() // حذف موارد تکراری
                    .ToListAsync();

                return Ok(warehousesWithStock);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ خطا در دریافت انبارهای دارای موجودی: {ex.Message}");
                return StatusCode(500, "خطا در دریافت اطلاعات انبارها");
            }
        }

    }
}