using AcxiomCRM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcxiomCRM.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AuditLogController : Controller
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        public async Task<IActionResult> Index(string? search, string? actionFilter)
        {
            var logs = await _auditLogService.GetAuditLogsAsync(search, actionFilter, 200);
            ViewBag.SearchTerm = search;
            ViewBag.ActionFilter = actionFilter;
            return View(logs);
        }
    }
}
