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
    [AuthorizeRole(Roles.Admin, Roles.SeniorUser, Roles.SeniorStorekeeper, Roles.Storekeeper)]
    public class StockOperationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditService _auditService;

        public StockOperationsController(ApplicationDbContext context, AuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        // POST: api/StockOperations/issue - خروج کالا
        [HttpPost("issue")]
        public async Task<ActionResult<StockOperationDto>> IssueStock(IssueStockDto issueDto)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var currentUserRoleId = int.Parse(User.FindFirst("RoleId")?.Value);
            var currentUserRole = Roles.GetRoleName(currentUserRoleId);

            // بررسی دسترسی به انبار
            if (currentUserRole == Roles.Storekeeper)
            {
                var canEdit = await _context.WarehouseAccesses
                    .AnyAsync(wa => wa.UserId == currentUserId &&
                                   wa.WarehouseId == issueDto.WarehouseId &&
                                   wa.CanEdit);

                if (!canEdit)
                {
                    return Forbid("شما مجوز خروج کالا از این انبار را ندارید");
                }
            }

            // بررسی موجودی کافی
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.ProductId == issueDto.ProductId &&
                                         i.WarehouseId == issueDto.WarehouseId &&
                                         i.BrandId == issueDto.BrandId);

            if (inventory == null || inventory.Quantity < issueDto.Quantity)
            {
                return BadRequest("موجودی کافی نیست");
            }

            // کسر از موجودی
            inventory.Quantity -= issueDto.Quantity;
            inventory.LastUpdated = DateTime.UtcNow;

            // ثبت عملیات خروج
            var stockOperation = new StockOperation
            {
                ProductId = issueDto.ProductId,
                WarehouseId = issueDto.WarehouseId,
                BrandId = issueDto.BrandId,
                Quantity = issueDto.Quantity,
                OperationType = "ISSUE",
                CostCenterId = issueDto.CostCenterId,
                Reason = issueDto.Reason,
                OperationDate = DateTime.UtcNow,
                CreatedBy = currentUserId,
                CreatedDate = DateTime.UtcNow
            };

            _context.StockOperations.Add(stockOperation);

            // به روزرسانی موجودی کل محصول
            await UpdateProductTotalQuantity(issueDto.ProductId);

            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync("StockOperations", stockOperation.Id, "CREATE",
                null,
                new { stockOperation.ProductId, stockOperation.Quantity, stockOperation.OperationType },
                $"خروج کالا از انبار - تعداد: {issueDto.Quantity}");

            var operationDto = await _context.StockOperations
                .Include(so => so.Product)
                .Include(so => so.Warehouse)
                .Include(so => so.Brand)
                .Include(so => so.CostCenter)
                .Include(so => so.CreatedByUser)
                .Where(so => so.Id == stockOperation.Id)
                .Select(so => new StockOperationDto
                {
                    Id = so.Id,
                    ProductId = so.ProductId,
                    ProductName = so.Product.Name,
                    WarehouseId = so.WarehouseId,
                    WarehouseName = so.Warehouse.Name,
                    BrandId = so.BrandId,
                    BrandName = so.Brand.Name,
                    Quantity = so.Quantity,
                    OperationType = so.OperationType,
                    CostCenterId = so.CostCenterId,
                    CostCenterName = so.CostCenter.Name,
                    Reason = so.Reason,
                    OperationDate = so.OperationDate,
                    CreatedBy = so.CreatedBy,
                    CreatedByName = $"{so.CreatedByUser.FirstName} {so.CreatedByUser.LastName}"
                })
                .FirstOrDefaultAsync();

            return Ok(operationDto);
        }

        // POST: api/StockOperations/receive - ورود کالا
        [HttpPost("receive")]
        public async Task<ActionResult<StockOperationDto>> ReceiveStock(ReceiveStockDto receiveDto)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var currentUserRoleId = int.Parse(User.FindFirst("RoleId")?.Value);
            var currentUserRole = Roles.GetRoleName(currentUserRoleId);

            // بررسی دسترسی به انبار
            if (currentUserRole == Roles.Storekeeper)
            {
                var canEdit = await _context.WarehouseAccesses
                    .AnyAsync(wa => wa.UserId == currentUserId &&
                                   wa.WarehouseId == receiveDto.WarehouseId &&
                                   wa.CanEdit);

                if (!canEdit)
                {
                    return Forbid("شما مجوز ورود کالا به این انبار را ندارید");
                }
            }

            // افزودن به موجودی
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.ProductId == receiveDto.ProductId &&
                                         i.WarehouseId == receiveDto.WarehouseId &&
                                         i.BrandId == receiveDto.BrandId);

            if (inventory == null)
            {
                // ایجاد رکورد جدید
                inventory = new Inventory
                {
                    ProductId = receiveDto.ProductId,
                    WarehouseId = receiveDto.WarehouseId,
                    BrandId = receiveDto.BrandId,
                    Quantity = receiveDto.Quantity,
                    LastUpdated = DateTime.UtcNow
                };
                _context.Inventories.Add(inventory);
            }
            else
            {
                inventory.Quantity += receiveDto.Quantity;
                inventory.LastUpdated = DateTime.UtcNow;
            }

            // ثبت عملیات ورود
            var stockOperation = new StockOperation
            {
                ProductId = receiveDto.ProductId,
                WarehouseId = receiveDto.WarehouseId,
                BrandId = receiveDto.BrandId,
                Quantity = receiveDto.Quantity,
                OperationType = "RECEIVE",
                Reason = receiveDto.Reason,
                OperationDate = DateTime.UtcNow,
                CreatedBy = currentUserId,
                CreatedDate = DateTime.UtcNow
            };

            _context.StockOperations.Add(stockOperation);

            // به روزرسانی موجودی کل محصول
            await UpdateProductTotalQuantity(receiveDto.ProductId);

            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync("StockOperations", stockOperation.Id, "CREATE",
                null,
                new { stockOperation.ProductId, stockOperation.Quantity, stockOperation.OperationType },
                $"ورود کالا به انبار - تعداد: {receiveDto.Quantity}");

            var operationDto = await _context.StockOperations
                .Include(so => so.Product)
                .Include(so => so.Warehouse)
                .Include(so => so.Brand)
                .Include(so => so.CreatedByUser)
                .Where(so => so.Id == stockOperation.Id)
                .Select(so => new StockOperationDto
                {
                    Id = so.Id,
                    ProductId = so.ProductId,
                    ProductName = so.Product.Name,
                    WarehouseId = so.WarehouseId,
                    WarehouseName = so.Warehouse.Name,
                    BrandId = so.BrandId,
                    BrandName = so.Brand.Name,
                    Quantity = so.Quantity,
                    OperationType = so.OperationType,
                    Reason = so.Reason,
                    OperationDate = so.OperationDate,
                    CreatedBy = so.CreatedBy,
                    CreatedByName = $"{so.CreatedByUser.FirstName} {so.CreatedByUser.LastName}"
                })
                .FirstOrDefaultAsync();

            return Ok(operationDto);
        }

        // GET: api/StockOperations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StockOperationDto>>> GetStockOperations(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] string operationType = null)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var currentUserRoleId = int.Parse(User.FindFirst("RoleId")?.Value);
                var currentUserRole = Roles.GetRoleName(currentUserRoleId);

                var query = _context.StockOperations
                    .Include(so => so.Product)
                    .Include(so => so.Warehouse)
                    .Include(so => so.Brand)
                    .Include(so => so.CostCenter)
                    .Include(so => so.CreatedByUser)
                    .AsQueryable();

                // 🔥 اضافه کردن فیلتر دسترسی برای انباردار
                if (currentUserRole == Roles.Storekeeper)
                {
                    var accessibleWarehouses = await _context.WarehouseAccesses
                        .Where(wa => wa.UserId == currentUserId && (wa.CanView || wa.CanEdit))
                        .Select(wa => wa.WarehouseId)
                        .ToListAsync();

                    query = query.Where(so => accessibleWarehouses.Contains(so.WarehouseId));
                }

                if (fromDate.HasValue)
                    query = query.Where(so => so.OperationDate >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(so => so.OperationDate <= toDate.Value);

                if (!string.IsNullOrEmpty(operationType))
                    query = query.Where(so => so.OperationType == operationType);

                var operations = await query
                    .OrderByDescending(so => so.OperationDate)
                    .Select(so => new StockOperationDto
                    {
                        Id = so.Id,
                        ProductId = so.ProductId,
                        ProductName = so.Product.Name,
                        ProductMainCode = so.Product.MainCode,
                        WarehouseId = so.WarehouseId,
                        WarehouseName = so.Warehouse.Name,
                        BrandId = so.BrandId,
                        BrandName = so.Brand.Name,
                        Quantity = so.Quantity,
                        OperationType = so.OperationType,
                        CostCenterId = so.CostCenterId,
                        CostCenterName = so.CostCenter != null ? so.CostCenter.Name : null,
                        Reason = so.Reason,
                        OperationDate = so.OperationDate,
                        CreatedBy = so.CreatedBy,
                        CreatedByName = $"{so.CreatedByUser.FirstName} {so.CreatedByUser.LastName}"
                    })
                    .ToListAsync();

                return Ok(operations);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ خطا در دریافت عملیات: {ex.Message}");
                return StatusCode(500, "خطا در دریافت تاریخچه عملیات");
            }
        }


        //// GET: api/StockOperations
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<StockOperationDto>>> GetStockOperations(
        //    [FromQuery] DateTime? fromDate = null,
        //    [FromQuery] DateTime? toDate = null,
        //    [FromQuery] string operationType = null)
        //{
        //    var query = _context.StockOperations
        //        .Include(so => so.Product)
        //        .Include(so => so.Warehouse)
        //        .Include(so => so.Brand)
        //        .Include(so => so.CostCenter)
        //        .Include(so => so.CreatedByUser)
        //        .AsQueryable();

        //    if (fromDate.HasValue)
        //        query = query.Where(so => so.OperationDate >= fromDate.Value);

        //    if (toDate.HasValue)
        //        query = query.Where(so => so.OperationDate <= toDate.Value);

        //    if (!string.IsNullOrEmpty(operationType))
        //        query = query.Where(so => so.OperationType == operationType);

        //    var operations = await query
        //        .OrderByDescending(so => so.OperationDate)
        //        .Select(so => new StockOperationDto
        //        {
        //            Id = so.Id,
        //            ProductId = so.ProductId,
        //            ProductName = so.Product.Name,
        //            WarehouseId = so.WarehouseId,
        //            WarehouseName = so.Warehouse.Name,
        //            BrandId = so.BrandId,
        //            BrandName = so.Brand.Name,
        //            Quantity = so.Quantity,
        //            OperationType = so.OperationType,
        //            CostCenterId = so.CostCenterId,
        //            CostCenterName = so.CostCenter != null ? so.CostCenter.Name : null,
        //            Reason = so.Reason,
        //            OperationDate = so.OperationDate,
        //            CreatedBy = so.CreatedBy,
        //            CreatedByName = $"{so.CreatedByUser.FirstName} {so.CreatedByUser.LastName}"
        //        })
        //        .ToListAsync();

        //    return Ok(operations);
        //}

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
    }
}