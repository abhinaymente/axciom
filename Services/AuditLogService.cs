using AcxiomCRM.Data;
using AcxiomCRM.Models;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly ApplicationDbContext _context;

        public AuditLogService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(string userId, string userName, string action, string entityName, string recordId, string? oldValue = null, string? newValue = null, string? ipAddress = null)
        {
            try
            {
                var auditLog = new AuditLog
                {
                    UserId = userId ?? "System",
                    UserName = string.IsNullOrWhiteSpace(userName) ? "System" : userName,
                    Action = action,
                    EntityName = entityName,
                    RecordId = recordId,
                    OldValue = oldValue,
                    NewValue = newValue,
                    CreatedDate = DateTime.UtcNow,
                    IpAddress = ipAddress ?? "127.0.0.1"
                };

                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();
            }
            catch
            {
                // Prevent audit log errors from crashing main transactions
            }
        }

        public async Task<List<AuditLog>> GetAuditLogsAsync(string? searchTerm = null, string? actionFilter = null, int limit = 100)
        {
            var query = _context.AuditLogs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(a => a.UserName.Contains(searchTerm) || 
                                         a.EntityName.Contains(searchTerm) || 
                                         a.RecordId.Contains(searchTerm) || 
                                         a.Action.Contains(searchTerm));
            }

            if (!string.IsNullOrWhiteSpace(actionFilter))
            {
                query = query.Where(a => a.Action == actionFilter);
            }

            return await query.OrderByDescending(a => a.CreatedDate)
                              .Take(limit)
                              .ToListAsync();
        }
    }
}
