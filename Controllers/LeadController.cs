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
    public class LeadController : Controller
    {
        private readonly ICrmService _crmService;
        private readonly UserManager<ApplicationUser> _userManager;

        public LeadController(ICrmService crmService, UserManager<ApplicationUser> userManager)
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

        public async Task<IActionResult> Index(string? search, string? status, int page = 1)
        {
            var (userId, role, _) = await GetCurrentUserInfoAsync();
            var leads = await _crmService.GetLeadsAsync(userId, role, search, status);

            int pageSize = 10;
            int totalCount = leads.Count;
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            var pagedLeads = leads.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var vm = new LeadListViewModel
            {
                Leads = pagedLeads,
                SearchTerm = search,
                StatusFilter = status,
                PageNumber = page,
                TotalPages = totalPages > 0 ? totalPages : 1
            };

            return View(vm);
        }

        public async Task<IActionResult> Details(int id)
        {
            var (userId, role, _) = await GetCurrentUserInfoAsync();
            var lead = await _crmService.GetLeadByIdAsync(id, userId, role);
            if (lead == null)
            {
                TempData["ErrorMessage"] = "Lead record not found or access denied.";
                return RedirectToAction(nameof(Index));
            }
            return View(lead);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Users = await _userManager.Users.Where(u => u.IsActive).ToListAsync();
            return View(new Lead());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Lead lead)
        {
            var (userId, role, userName) = await GetCurrentUserInfoAsync();

            if (!ModelState.IsValid)
            {
                ViewBag.Users = await _userManager.Users.Where(u => u.IsActive).ToListAsync();
                return View(lead);
            }

            var (success, errorMessage) = await _crmService.CreateLeadAsync(lead, userId, userName);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage);
                ViewBag.Users = await _userManager.Users.Where(u => u.IsActive).ToListAsync();
                return View(lead);
            }

            TempData["SuccessMessage"] = "Lead created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var (userId, role, _) = await GetCurrentUserInfoAsync();
            var lead = await _crmService.GetLeadByIdAsync(id, userId, role);
            if (lead == null)
            {
                TempData["ErrorMessage"] = "Lead record not found or access denied.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Users = await _userManager.Users.Where(u => u.IsActive).ToListAsync();
            return View(lead);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Lead lead)
        {
            var (userId, role, userName) = await GetCurrentUserInfoAsync();

            if (id != lead.LeadId)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Users = await _userManager.Users.Where(u => u.IsActive).ToListAsync();
                return View(lead);
            }

            var (success, errorMessage) = await _crmService.UpdateLeadAsync(lead, userId, role, userName);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage);
                ViewBag.Users = await _userManager.Users.Where(u => u.IsActive).ToListAsync();
                return View(lead);
            }

            TempData["SuccessMessage"] = "Lead updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var (userId, role, userName) = await GetCurrentUserInfoAsync();
            var (success, errorMessage) = await _crmService.DeleteLeadAsync(id, userId, role, userName);

            if (!success)
            {
                TempData["ErrorMessage"] = errorMessage;
            }
            else
            {
                TempData["SuccessMessage"] = "Lead deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Convert(int id)
        {
            var (userId, role, _) = await GetCurrentUserInfoAsync();
            var lead = await _crmService.GetLeadByIdAsync(id, userId, role);
            if (lead == null)
            {
                TempData["ErrorMessage"] = "Lead record not found or access denied.";
                return RedirectToAction(nameof(Index));
            }

            if (lead.Status == "Converted")
            {
                TempData["ErrorMessage"] = "This lead is already converted.";
                return RedirectToAction(nameof(Index));
            }

            var vm = new LeadConvertViewModel
            {
                LeadId = lead.LeadId,
                LeadName = lead.LeadName,
                Email = lead.Email,
                Phone = lead.Phone,
                CompanyName = lead.CompanyName,
                CustomerName = lead.CompanyName ?? lead.LeadName,
                OpportunityName = $"{lead.LeadName} Deal",
                OpportunityAmount = lead.ExpectedValue > 0 ? lead.ExpectedValue : 10000m,
                Stage = "Qualification",
                Probability = 50,
                ExpectedCloseDate = DateTime.Today.AddDays(30)
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Convert(LeadConvertViewModel model)
        {
            var (userId, role, userName) = await GetCurrentUserInfoAsync();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (success, errorMessage) = await _crmService.ConvertLeadAsync(model, userId, userName);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage);
                return View(model);
            }

            TempData["SuccessMessage"] = "Lead converted successfully into Customer and/or Opportunity!";
            return RedirectToAction(nameof(Index));
        }
    }
}
