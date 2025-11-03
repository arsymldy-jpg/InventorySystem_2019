using System;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Inventory_Api.Data;
using Inventory_Api.Models.Entities;
using Inventory_Api.Models.DTOs;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inventory_Api.Services
{
    public class AuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogActionAsync(string tableName, int recordId, string action, object oldValues = null, object newValues = null, string description = null)
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

            var auditLog = new AuditLog
            {
                TableName = tableName,
                RecordId = recordId,
                Action = action,
                OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
                NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
                Description = description,
                UserId = userId != null ? int.Parse(userId) : 0,
                IpAddress = ipAddress,
                Timestamp = DateTime.UtcNow
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }

        public async Task<List<AuditLogDto>> GetAuditLogsAsync(string tableName = null, int? recordId = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.AuditLogs
                .Include(al => al.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(tableName))
                query = query.Where(al => al.TableName == tableName);

            if (recordId.HasValue)
                query = query.Where(al => al.RecordId == recordId.Value);

            if (fromDate.HasValue)
                query = query.Where(al => al.Timestamp >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(al => al.Timestamp <= toDate.Value);

            var logs = await query
                .OrderByDescending(al => al.Timestamp)
                .Select(al => new AuditLogDto
                {
                    Id = al.Id,
                    TableName = al.TableName,
                    RecordId = al.RecordId,
                    Action = al.Action,
                    OldValues = al.OldValues,
                    NewValues = al.NewValues,
                    Description = al.Description,
                    UserName = $"{al.User.FirstName} {al.User.LastName}",
                    UserId = al.UserId,
                    Timestamp = al.Timestamp,
                    IpAddress = al.IpAddress
                })
                .ToListAsync();

            return logs;
        }
    }
}