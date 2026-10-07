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
    public class ActivityController : Controller
    {
        private readonly ICrmService _crmService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ActivityController(ICrmService crmService, UserManager<ApplicationUser> userManager)
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

        public async Task<IActionResult> Index(string? search, string? type, int page = 1)
        {
            var (userId, role, _) = await GetCurrentUserInfoAsync();
            var activities = await _crmService.GetActivitiesAsync(userId, role, search, type);

            int pageSize = 10;
            int totalCount = activities.Count;
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            var pagedActivities = activities.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var vm = new ActivityListViewModel
            {
                Activities = pagedActivities,
                SearchTerm = search,
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
            return View(new Activity { ActivityDate = DateTime.Now });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Activity activity)
        {
            var (userId, role, userName) = await GetCurrentUserInfoAsync();

            if (!ModelState.IsValid)
            {
                ViewBag.Customers = await _crmService.GetCustomersAsync(userId, role);
                ViewBag.Leads = await _crmService.GetLeadsAsync(userId, role);
                ViewBag.Users = await _userManager.Users.Where(u => u.IsActive).ToListAsync();
                return View(activity);
            }

            var (success, errorMessage) = await _crmService.CreateActivityAsync(activity, userId, userName);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage);
                ViewBag.Customers = await _crmService.GetCustomersAsync(userId, role);
                ViewBag.Leads = await _crmService.GetLeadsAsync(userId, role);
                ViewBag.Users = await _userManager.Users.Where(u => u.IsActive).ToListAsync();
                return View(activity);
            }

            TempData["SuccessMessage"] = "Activity logged successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
