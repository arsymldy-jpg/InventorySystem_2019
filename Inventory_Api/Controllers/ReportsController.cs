using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Inventory_Api.Data;
using Inventory_Api.Models.DTOs;
using Inventory_Api.Helpers;

namespace Inventory_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AuthorizeRole(Roles.Admin, Roles.SeniorUser, Roles.SeniorStorekeeper, Roles.Storekeeper, Roles.Viewer)]
    public class ReportsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Reports/inventory-summary
        [HttpGet("inventory-summary")]
        public async Task<ActionResult<InventorySummaryReport>> GetInventorySummary()
        {
            var totalProducts = await _context.Products.CountAsync(p => p.IsActive);
            var totalWarehouses = await _context.Warehouses.CountAsync(w => w.IsActive);
            var totalUsers = await _context.Users.CountAsync(u => u.IsActive);

            var lowStockProducts = await _context.Products
                .Where(p => p.IsActive && p.TotalQuantity <= p.ReorderPoint)
                .CountAsync();

            var totalInventoryValue = await _context.Inventories
                .Include(i => i.Product)
                .SumAsync(i => i.Quantity); // می‌توانید قیمت هم اضافه کنید

            return new InventorySummaryReport
            {
                TotalProducts = totalProducts,
                TotalWarehouses = totalWarehouses,
                TotalUsers = totalUsers,
                LowStockProducts = lowStockProducts,
                TotalInventoryValue = totalInventoryValue,
                GeneratedAt = DateTime.UtcNow
            };
        }

        // GET: api/Reports/warehouse-inventory
        [HttpGet("warehouse-inventory")]
        public async Task<ActionResult<IEnumerable<WarehouseInventoryReport>>> GetWarehouseInventoryReport()
        {
            var report = await _context.Warehouses
                .Where(w => w.IsActive)
                .Select(w => new WarehouseInventoryReport
                {
                    WarehouseId = w.Id,
                    WarehouseName = w.Name,
                    TotalProducts = _context.Inventories.Count(i => i.WarehouseId == w.Id),
                    TotalQuantity = _context.Inventories.Where(i => i.WarehouseId == w.Id).Sum(i => i.Quantity),
                    LastUpdated = _context.Inventories
                        .Where(i => i.WarehouseId == w.Id)
                        .Max(i => (DateTime?)i.LastUpdated) ?? w.CreatedDate
                })
                .ToListAsync();

            return Ok(report);
        }

        // GET: api/Reports/brand-summary
        [HttpGet("brand-summary")]
        public async Task<ActionResult<IEnumerable<BrandSummaryReport>>> GetBrandSummaryReport()
        {
            var report = await _context.Brands
                .Where(b => b.IsActive)
                .Select(b => new BrandSummaryReport
                {
                    BrandId = b.Id,
                    BrandName = b.Name,
                    TotalProducts = _context.ProductBrands.Count(pb => pb.BrandId == b.Id),
                    TotalQuantity = _context.Inventories
                        .Where(i => i.BrandId == b.Id)
                        .Sum(i => i.Quantity),
                    WarehousesCount = _context.Inventories
                        .Where(i => i.BrandId == b.Id)
                        .Select(i => i.WarehouseId)
                        .Distinct()
                        .Count()
                })
                .ToListAsync();

            return Ok(report);
        }

        // GET: api/Reports/low-stock-alerts
        [HttpGet("low-stock-alerts")]
        [AuthorizeRole(Roles.Admin, Roles.SeniorUser, Roles.SeniorStorekeeper, Roles.Storekeeper)]
        public async Task<ActionResult<IEnumerable<LowStockAlertReport>>> GetLowStockAlerts()
        {
            var alerts = await _context.Products
                .Where(p => p.IsActive && p.TotalQuantity <= p.ReorderPoint)
                .Select(p => new LowStockAlertReport
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    MainCode = p.MainCode,
                    CurrentQuantity = p.TotalQuantity,
                    ReorderPoint = p.ReorderPoint,
                    SafetyStock = p.SafetyStock,
                    ShortageAmount = p.ReorderPoint - p.TotalQuantity,
                    AlertLevel = p.TotalQuantity <= p.SafetyStock ? "CRITICAL" : "WARNING"
                })
                .OrderBy(a => a.AlertLevel)
                .ThenBy(a => a.ShortageAmount)
                .ToListAsync();

            return Ok(alerts);
        }

        // GET: api/Reports/user-activity
        [HttpGet("user-activity")]
        [AuthorizeRole(Roles.Admin, Roles.SeniorUser)]
        public async Task<ActionResult<IEnumerable<UserActivityReport>>> GetUserActivityReport([FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            var startDate = fromDate ?? DateTime.UtcNow.AddDays(-30);
            var endDate = toDate ?? DateTime.UtcNow;

            var report = await _context.Users
                .Where(u => u.IsActive)
                .Select(u => new UserActivityReport
                {
                    UserId = u.Id,
                    UserName = $"{u.FirstName} {u.LastName}",
                    RoleName = u.Role.Name,
                    LastLogin = u.LastLogin,
                    LoginCount = _context.AuditLogs
                        .Count(al => al.UserId == u.Id &&
                                    al.Action == "LOGIN" &&
                                    al.Timestamp >= startDate &&
                                    al.Timestamp <= endDate),
                    ActionsCount = _context.AuditLogs
                        .Count(al => al.UserId == u.Id &&
                                    al.Timestamp >= startDate &&
                                    al.Timestamp <= endDate)
                })
                .OrderByDescending(r => r.LastLogin)
                .ToListAsync();

            return Ok(report);
        }

        // GET: api/Reports/brand-inventory
        [HttpGet("brand-inventory")]
        public async Task<ActionResult<IEnumerable<BrandInventoryReport>>> GetBrandInventoryReport()
        {
            var report = await _context.Brands
                .Where(b => b.IsActive)
                .Select(b => new BrandInventoryReport
                {
                    BrandId = b.Id,
                    BrandName = b.Name,
                    TotalProducts = _context.ProductBrands.Count(pb => pb.BrandId == b.Id),
                    TotalQuantity = _context.Inventories
                        .Where(i => i.BrandId == b.Id)
                        .Sum(i => i.Quantity),
                    WarehouseDistribution = _context.Inventories
                        .Where(i => i.BrandId == b.Id)
                        .GroupBy(i => i.WarehouseId)
                        .Select(g => new WarehouseQuantity
                        {
                            WarehouseId = g.Key,
                            WarehouseName = g.First().Warehouse.Name,
                            Quantity = g.Sum(x => x.Quantity)
                        })
                        .ToList()
                })
                .Where(r => r.TotalQuantity > 0) // فقط برندهایی که موجودی دارند
                .OrderByDescending(r => r.TotalQuantity)
                .ToListAsync();

            return Ok(report);
        }

        // GET: api/Reports/product-brand-summary
        [HttpGet("product-brand-summary")]
        public async Task<ActionResult<IEnumerable<ProductBrandSummaryReport>>> GetProductBrandSummaryReport()
        {
            var report = await _context.Products
                .Where(p => p.IsActive)
                .Select(p => new ProductBrandSummaryReport
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    MainCode = p.MainCode,
                    TotalQuantity = p.TotalQuantity,
                    Brands = _context.ProductBrands
                        .Where(pb => pb.ProductId == p.Id)
                        .Select(pb => new BrandQuantity
                        {
                            BrandId = pb.BrandId,
                            BrandName = pb.Brand.Name,
                            TotalQuantity = _context.Inventories
                                .Where(i => i.ProductId == p.Id && i.BrandId == pb.BrandId)
                                .Sum(i => i.Quantity)
                        })
                        .Where(bq => bq.TotalQuantity > 0)
                        .ToList()
                })
                .Where(p => p.Brands.Any()) // فقط محصولاتی که برند دارند
                .OrderByDescending(p => p.TotalQuantity)
                .ToListAsync();

            return Ok(report);
        }
    }
}