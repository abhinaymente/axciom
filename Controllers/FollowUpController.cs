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
    public class FollowUpController : Controller
    {
        private readonly ICrmService _crmService;
        private readonly UserManager<ApplicationUser> _userManager;

        public FollowUpController(ICrmService crmService, UserManager<ApplicationUser> userManager)
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

        public async Task<IActionResult> Index(string? search, string? status, string? type, int page = 1)
        {
            var (userId, role, _) = await GetCurrentUserInfoAsync();
            var followUps = await _crmService.GetFollowUpsAsync(userId, role, search, status, type);

            int pageSize = 10;
            int totalCount = followUps.Count;
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            var pagedFollowUps = followUps.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var vm = new FollowUpListViewModel
            {
                FollowUps = pagedFollowUps,
                SearchTerm = search,
                StatusFilter = status,
                TypeFilter = type,
                PageNumber = page,
                TotalPages = totalPages > 0 ? totalPages : 1
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var (userId, role, _) = await GetCurrentUserInfoAsync();
            ViewBag.Customers = await _crmService.GetCustomersAsync(userId, role);
            ViewBag.Leads = await _crmService.GetLeadsAsync(userId, role);
            ViewBag.Users = await _userManager.Users.Where(u => u.IsActive).ToListAsync();
            return View(new FollowUp { FollowUpDate = DateTime.Now.AddDays(1) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FollowUp followUp)
        {
            var (userId, role, userName) = await GetCurrentUserInfoAsync();

            if (!ModelState.IsValid)
            {
                ViewBag.Customers = await _crmService.GetCustomersAsync(userId, role);
                ViewBag.Leads = await _crmService.GetLeadsAsync(userId, role);
                ViewBag.Users = await _userManager.Users.Where(u => u.IsActive).ToListAsync();
                return View(followUp);
            }

            var (success, errorMessage) = await _crmService.CreateFollowUpAsync(followUp, userId, userName);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage);
                ViewBag.Customers = await _crmService.GetCustomersAsync(userId, role);
                ViewBag.Leads = await _crmService.GetLeadsAsync(userId, role);
                ViewBag.Users = await _userManager.Users.Where(u => u.IsActive).ToListAsync();
                return View(followUp);
            }

            TempData["SuccessMessage"] = "Follow-Up scheduled successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var (userId, role, _) = await GetCurrentUserInfoAsync();
            var followUp = await _crmService.GetFollowUpByIdAsync(id, userId, role);
            if (followUp == null)
            {
                TempData["ErrorMessage"] = "Follow-up record not found or access denied.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Customers = await _crmService.GetCustomersAsync(userId, role);
            ViewBag.Leads = await _crmService.GetLeadsAsync(userId, role);
            ViewBag.Users = await _userManager.Users.Where(u => u.IsActive).ToListAsync();
            return View(followUp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FollowUp followUp)
        {
            var (userId, role, userName) = await GetCurrentUserInfoAsync();

            if (id != followUp.FollowUpId)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Customers = await _crmService.GetCustomersAsync(userId, role);
                ViewBag.Leads = await _crmService.GetLeadsAsync(userId, role);
                ViewBag.Users = await _userManager.Users.Where(u => u.IsActive).ToListAsync();
                return View(followUp);
            }

            var (success, errorMessage) = await _crmService.UpdateFollowUpAsync(followUp, userId, role, userName);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage);
                ViewBag.Customers = await _crmService.GetCustomersAsync(userId, role);
                ViewBag.Leads = await _crmService.GetLeadsAsync(userId, role);
                ViewBag.Users = await _userManager.Users.Where(u => u.IsActive).ToListAsync();
                return View(followUp);
            }

            TempData["SuccessMessage"] = "Follow-Up updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var (userId, role, userName) = await GetCurrentUserInfoAsync();
            var (success, errorMessage) = await _crmService.DeleteFollowUpAsync(id, userId, role, userName);

            if (!success)
            {
                TempData["ErrorMessage"] = errorMessage;
            }
            else
            {
                TempData["SuccessMessage"] = "Follow-Up deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
