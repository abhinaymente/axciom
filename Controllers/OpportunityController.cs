using AcxiomCRM.Models;
using AcxiomCRM.Services;
using AcxiomCRM.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Controllers
{
    [Authorize]
    public class OpportunityController : Controller
    {
        private readonly ICrmService _crmService;
        private readonly UserManager<ApplicationUser> _userManager;

        public OpportunityController(ICrmService crmService, UserManager<ApplicationUser> userManager)
        {
            _crmService = crmService;
            _userManager = userManager;
        }

        private async Task<(string UserId, string PrimaryRole, string UserName)> GetCurrentUserInfoAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user!);
            return (user!.Id, roles.FirstOrDefault() ?? "SalesExecutive", user.Email ?? "User");
        }

        public async Task<IActionResult> Index(string? search, string? stage, string? status, int page = 1)
        {
            var (userId, role, _) = await GetCurrentUserInfoAsync();
            var opportunities = await _crmService.GetOpportunitiesAsync(userId, role, search, stage, status);

            int pageSize = 10;
            int totalCount = opportunities.Count;
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            var pagedOpps = opportunities.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var vm = new OpportunityListViewModel
            {
                Opportunities = pagedOpps,
                SearchTerm = search,
                StageFilter = stage,
                StatusFilter = status,
                TotalPipelineValue = opportunities.Where(o => o.Status == "Active").Sum(o => o.Amount),
                PageNumber = page,
                TotalPages = totalPages > 0 ? totalPages : 1
            };

            return View(vm);
        }

        public async Task<IActionResult> Details(int id)
        {
            var (userId, role, _) = await GetCurrentUserInfoAsync();
            var opp = await _crmService.GetOpportunityByIdAsync(id, userId, role);
            if (opp == null)
            {
                TempData["ErrorMessage"] = "Opportunity record not found or access denied.";
                return RedirectToAction(nameof(Index));
            }
            return View(opp);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var (userId, role, _) = await GetCurrentUserInfoAsync();
            ViewBag.Customers = await _crmService.GetCustomersAsync(userId, role);
            ViewBag.Leads = await _crmService.GetLeadsAsync(userId, role);
            ViewBag.Users = await _userManager.Users.Where(u => u.IsActive).ToListAsync();
            return View(new Opportunity());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Opportunity opportunity)
        {
            var (userId, role, userName) = await GetCurrentUserInfoAsync();

            if (!ModelState.IsValid)
            {
                ViewBag.Customers = await _crmService.GetCustomersAsync(userId, role);
                ViewBag.Leads = await _crmService.GetLeadsAsync(userId, role);
                ViewBag.Users = await _userManager.Users.Where(u => u.IsActive).ToListAsync();
                return View(opportunity);
            }

            var (success, errorMessage) = await _crmService.CreateOpportunityAsync(opportunity, userId, userName);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage);
                ViewBag.Customers = await _crmService.GetCustomersAsync(userId, role);
                ViewBag.Leads = await _crmService.GetLeadsAsync(userId, role);
                ViewBag.Users = await _userManager.Users.Where(u => u.IsActive).ToListAsync();
                return View(opportunity);
            }

            TempData["SuccessMessage"] = "Opportunity created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var (userId, role, _) = await GetCurrentUserInfoAsync();
            var opp = await _crmService.GetOpportunityByIdAsync(id, userId, role);
            if (opp == null)
            {
                TempData["ErrorMessage"] = "Opportunity record not found or access denied.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Customers = await _crmService.GetCustomersAsync(userId, role);
            ViewBag.Leads = await _crmService.GetLeadsAsync(userId, role);
            ViewBag.Users = await _userManager.Users.Where(u => u.IsActive).ToListAsync();
            return View(opp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Opportunity opportunity)
        {
            var (userId, role, userName) = await GetCurrentUserInfoAsync();

            if (id != opportunity.OpportunityId)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Customers = await _crmService.GetCustomersAsync(userId, role);
                ViewBag.Leads = await _crmService.GetLeadsAsync(userId, role);
                ViewBag.Users = await _userManager.Users.Where(u => u.IsActive).ToListAsync();
                return View(opportunity);
            }

            var (success, errorMessage) = await _crmService.UpdateOpportunityAsync(opportunity, userId, role, userName);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage);
                ViewBag.Customers = await _crmService.GetCustomersAsync(userId, role);
                ViewBag.Leads = await _crmService.GetLeadsAsync(userId, role);
                ViewBag.Users = await _userManager.Users.Where(u => u.IsActive).ToListAsync();
                return View(opportunity);
            }

            TempData["SuccessMessage"] = "Opportunity updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var (userId, role, userName) = await GetCurrentUserInfoAsync();
            var (success, errorMessage) = await _crmService.DeleteOpportunityAsync(id, userId, role, userName);

            if (!success)
            {
                TempData["ErrorMessage"] = errorMessage;
            }
            else
            {
                TempData["SuccessMessage"] = "Opportunity deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
