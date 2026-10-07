using AcxiomCRM.Models;
using AcxiomCRM.Models.DTOs;
using AcxiomCRM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AcxiomCRM.Controllers.Api
{
    [Route("api/customers")]
    [ApiController]
    [Authorize]
    public class CustomersApiController : ControllerBase
    {
        private readonly ICrmService _crmService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CustomersApiController(ICrmService crmService, UserManager<ApplicationUser> userManager)
        {
            _crmService = crmService;
            _userManager = userManager;
        }

        private async Task<(string UserId, string Role, string UserName)> GetCurrentUserInfoAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return ("System", "SalesExecutive", "System");
            var roles = await _userManager.GetRolesAsync(user);
            return (user.Id, roles.FirstOrDefault() ?? "SalesExecutive", user.Email ?? "User");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> GetCustomers([FromQuery] string? search, [FromQuery] string? status)
        {
            var (userId, role, _) = await GetCurrentUserInfoAsync();
            var customers = await _crmService.GetCustomersAsync(userId, role, search, status);

            var dtos = customers.Select(c => new CustomerDto
            {
                CustomerId = c.CustomerId,
                CustomerCode = c.CustomerCode,
                CustomerName = c.CustomerName,
                Email = c.Email,
                Phone = c.Phone,
                CompanyName = c.CompanyName,
                Address = c.Address,
                City = c.City,
                State = c.State,
                Status = c.Status,
                CreatedDate = c.CreatedDate
            });

            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerDto>> GetCustomer(int id)
        {
            var (userId, role, _) = await GetCurrentUserInfoAsync();
            var c = await _crmService.GetCustomerByIdAsync(id, userId, role);
            if (c == null)
            {
                return NotFound(new { message = "Customer record not found or access denied." });
            }

            var dto = new CustomerDto
            {
                CustomerId = c.CustomerId,
                CustomerCode = c.CustomerCode,
                CustomerName = c.CustomerName,
                Email = c.Email,
                Phone = c.Phone,
                CompanyName = c.CompanyName,
                Address = c.Address,
                City = c.City,
                State = c.State,
                Status = c.Status,
                CreatedDate = c.CreatedDate
            };

            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult<CustomerDto>> CreateCustomer([FromBody] CustomerDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (userId, role, userName) = await GetCurrentUserInfoAsync();

            var customer = new Customer
            {
                CustomerCode = dto.CustomerCode,
                CustomerName = dto.CustomerName,
                Email = dto.Email,
                Phone = dto.Phone,
                CompanyName = dto.CompanyName,
                Address = dto.Address,
                City = dto.City,
                State = dto.State,
                Status = string.IsNullOrWhiteSpace(dto.Status) ? "Active" : dto.Status
            };

            var (success, errorMessage) = await _crmService.CreateCustomerAsync(customer, userId, userName);
            if (!success)
            {
                return BadRequest(new { message = errorMessage });
            }

            dto.CustomerId = customer.CustomerId;
            dto.CustomerCode = customer.CustomerCode;
            dto.CreatedDate = customer.CreatedDate;

            return CreatedAtAction(nameof(GetCustomer), new { id = customer.CustomerId }, dto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] CustomerDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (userId, role, userName) = await GetCurrentUserInfoAsync();

            var customer = new Customer
            {
                CustomerId = id,
                CustomerCode = dto.CustomerCode,
                CustomerName = dto.CustomerName,
                Email = dto.Email,
                Phone = dto.Phone,
                CompanyName = dto.CompanyName,
                Address = dto.Address,
                City = dto.City,
                State = dto.State,
                Status = dto.Status
            };

            var (success, errorMessage) = await _crmService.UpdateCustomerAsync(customer, userId, role, userName);
            if (!success)
            {
                return BadRequest(new { message = errorMessage });
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var (userId, role, userName) = await GetCurrentUserInfoAsync();
            var (success, errorMessage) = await _crmService.DeleteCustomerAsync(id, userId, role, userName);

            if (!success)
            {
                return BadRequest(new { message = errorMessage });
            }

            return NoContent();
        }
    }
}
