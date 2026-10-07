using AcxiomCRM.Models;
using AcxiomCRM.Services;
using AcxiomCRM.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AcxiomCRM.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IAuditLogService _auditLogService;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IAuditLogService auditLogService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _auditLogService = auditLogService;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                if (!user.IsActive)
                {
                    ModelState.AddModelError(string.Empty, "Your account has been deactivated. Please contact an administrator.");
                    await _auditLogService.LogAsync(user.Id, user.Email!, "FAILED_LOGIN", "Account", user.Id, null, "Deactivated account attempt", HttpContext.Connection.RemoteIpAddress?.ToString());
                    return View(model);
                }

                var result = await _signInManager.PasswordSignInAsync(user.UserName!, model.Password, model.RememberMe, lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    await _auditLogService.LogAsync(user.Id, user.Email!, "LOGIN", "Account", user.Id, null, "Successful login", HttpContext.Connection.RemoteIpAddress?.ToString());

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Index", "Home");
                }

                if (result.IsLockedOut)
                {
                    await _auditLogService.LogAsync(user.Id, user.Email!, "FAILED_LOGIN", "Account", user.Id, null, "Account Locked Out", HttpContext.Connection.RemoteIpAddress?.ToString());
                    ModelState.AddModelError(string.Empty, "Account locked out due to multiple failed attempts. Try again later.");
                    return View(model);
                }
            }

            await _auditLogService.LogAsync("Anonymous", model.Email, "FAILED_LOGIN", "Account", "N/A", null, "Invalid Credentials", HttpContext.Connection.RemoteIpAddress?.ToString());
            ModelState.AddModelError(string.Empty, "Invalid login attempt. Please check your email and password.");
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                Department = model.Department,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                string role = string.IsNullOrWhiteSpace(model.Role) ? "SalesExecutive" : model.Role;
                await _userManager.AddToRoleAsync(user, role);

                await _auditLogService.LogAsync(user.Id, user.Email, "REGISTER", "Account", user.Id, null, $"Role: {role}");
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var userId = _userManager.GetUserId(User);
            var userName = User.Identity?.Name ?? "User";

            await _signInManager.SignOutAsync();
            if (userId != null)
            {
                await _auditLogService.LogAsync(userId, userName, "LOGOUT", "Account", userId, null, "User logged out");
            }

            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
