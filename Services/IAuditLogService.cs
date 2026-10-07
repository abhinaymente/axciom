using AcxiomCRM.Models;

namespace AcxiomCRM.Services
{
    public interface IAuditLogService
    {
        Task LogAsync(string userId, string userName, string action, string entityName, string recordId, string? oldValue = null, string? newValue = null, string? ipAddress = null);
        Task<List<AuditLog>> GetAuditLogsAsync(string? searchTerm = null, string? actionFilter = null, int limit = 100);
    }
}
