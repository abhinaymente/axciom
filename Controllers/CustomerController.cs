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
    public class CustomerController : Controller
    {
        private readonly ICrmService _crmService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CustomerController(ICrmService crmService, UserManager<ApplicationUser> userManager)
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
            var customers = await _crmService.GetCustomersAsync(userId, role, search, status);

            int pageSize = 10;
            int totalCount = customers.Count;
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            var pagedCustomers = customers.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var vm = new CustomerListViewModel
            {
                Customers = pagedCustomers,
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
            var customer = await _crmService.GetCustomerByIdAsync(id, userId, role);
            if (customer == null)
            {
                TempData["ErrorMessage"] = "Customer record not found or access denied.";
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var users = await _userManager.Users.Where(u => u.IsActive).ToListAsync();
            ViewBag.Users = users;
            return View(new Customer());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)
        {
            var (userId, role, userName) = await GetCurrentUserInfoAsync();

            if (!ModelState.IsValid)
            {
                ViewBag.Users = await _userManager.Users.Where(u => u.IsActive).ToListAsync();
                return View(customer);
            }

            var (success, errorMessage) = await _crmService.CreateCustomerAsync(customer, userId, userName);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage);
                ViewBag.Users = await _userManager.Users.Where(u => u.IsActive).ToListAsync();
                return View(customer);
            }

            TempData["SuccessMessage"] = "Customer created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var (userId, role, _) = await GetCurrentUserInfoAsync();
            var customer = await _crmService.GetCustomerByIdAsync(id, userId, role);
            if (customer == null)
            {
                TempData["ErrorMessage"] = "Customer record not found or access denied.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Users = await _userManager.Users.Where(u => u.IsActive).ToListAsync();
            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Customer customer)
        {
            var (userId, role, userName) = await GetCurrentUserInfoAsync();

            if (id != customer.CustomerId)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Users = await _userManager.Users.Where(u => u.IsActive).ToListAsync();
                return View(customer);
            }

            var (success, errorMessage) = await _crmService.UpdateCustomerAsync(customer, userId, role, userName);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage);
                ViewBag.Users = await _userManager.Users.Where(u => u.IsActive).ToListAsync();
                return View(customer);
            }

            TempData["SuccessMessage"] = "Customer updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var (userId, role, userName) = await GetCurrentUserInfoAsync();
            var (success, errorMessage) = await _crmService.DeleteCustomerAsync(id, userId, role, userName);

            if (!success)
            {
                TempData["ErrorMessage"] = errorMessage;
            }
            else
            {
                TempData["SuccessMessage"] = "Customer deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
