using AcxiomCRM.Data;
using AcxiomCRM.Models;
using AcxiomCRM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly ICrmService _crmService;
        private readonly IAuditLogService _auditLogService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ReportController(
            ICrmService crmService,
            IAuditLogService auditLogService,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _crmService = crmService;
            _auditLogService = auditLogService;
            _userManager = userManager;
            _context = context;
        }

        private async Task<(string UserId, string PrimaryRole)> GetCurrentUserInfoAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user!);
            return (user!.Id, roles.FirstOrDefault() ?? "SalesExecutive");
        }

        public async Task<IActionResult> Index(string reportType = "Customer", string? search = null, string? status = null)
        {
            var (userId, role) = await GetCurrentUserInfoAsync();
            ViewBag.ReportType = reportType;
            ViewBag.SearchTerm = search;
            ViewBag.StatusFilter = status;

            switch (reportType)
            {
                case "Customer":
                    var customers = await _crmService.GetCustomersAsync(userId, role, search, status);
                    return View("CustomerReport", customers);

                case "Lead":
                    var leads = await _crmService.GetLeadsAsync(userId, role, search, status);
                    return View("LeadReport", leads);

                case "Opportunity":
                    var opps = await _crmService.GetOpportunitiesAsync(userId, role, search, null, status);
                    return View("OpportunityReport", opps);

                case "Pipeline":
                    var pipelineOpps = await _crmService.GetOpportunitiesAsync(userId, role, search, status, "Active");
                    return View("PipelineReport", pipelineOpps);

                case "FollowUp":
                    var followUps = await _crmService.GetFollowUpsAsync(userId, role, search, status);
                    return View("FollowUpReport", followUps);

                case "Conversion":
                    var convertedLeads = await _crmService.GetLeadsAsync(userId, role, search, "Converted");
                    return View("ConversionReport", convertedLeads);

                case "UserActivity":
                    var activities = await _crmService.GetActivitiesAsync(userId, role, search);
                    return View("UserActivityReport", activities);

                case "Audit":
                    if (role != "Admin")
                    {
                        TempData["ErrorMessage"] = "Audit report access is restricted to Administrators.";
                        return RedirectToAction(nameof(Index), new { reportType = "Customer" });
                    }
                    var logs = await _auditLogService.GetAuditLogsAsync(search, status);
                    return View("AuditReport", logs);

                default:
                    var defaultCustomers = await _crmService.GetCustomersAsync(userId, role, search, status);
                    return View("CustomerReport", defaultCustomers);
            }
        }
    }
}
