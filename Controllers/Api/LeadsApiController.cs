using AcxiomCRM.Models;
using AcxiomCRM.Models.DTOs;
using AcxiomCRM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AcxiomCRM.Controllers.Api
{
    [Route("api/leads")]
    [ApiController]
    [Authorize]
    public class LeadsApiController : ControllerBase
    {
        private readonly ICrmService _crmService;
        private readonly UserManager<ApplicationUser> _userManager;

        public LeadsApiController(ICrmService crmService, UserManager<ApplicationUser> userManager)
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
        public async Task<ActionResult<IEnumerable<LeadDto>>> GetLeads([FromQuery] string? search, [FromQuery] string? status)
        {
            var (userId, role, _) = await GetCurrentUserInfoAsync();
            var leads = await _crmService.GetLeadsAsync(userId, role, search, status);

            var dtos = leads.Select(l => new LeadDto
            {
                LeadId = l.LeadId,
                LeadCode = l.LeadCode,
                LeadName = l.LeadName,
                Email = l.Email,
                Phone = l.Phone,
                CompanyName = l.CompanyName,
                Source = l.Source,
                Status = l.Status,
                ExpectedValue = l.ExpectedValue,
                CreatedDate = l.CreatedDate
            });

            return Ok(dtos);
        }

        [HttpPost]
        public async Task<ActionResult<LeadDto>> CreateLead([FromBody] LeadDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (userId, role, userName) = await GetCurrentUserInfoAsync();

            var lead = new Lead
            {
                LeadCode = dto.LeadCode,
                LeadName = dto.LeadName,
                Email = dto.Email,
                Phone = dto.Phone,
                CompanyName = dto.CompanyName,
                Source = string.IsNullOrWhiteSpace(dto.Source) ? "Website" : dto.Source,
                Status = string.IsNullOrWhiteSpace(dto.Status) ? "New" : dto.Status,
                ExpectedValue = dto.ExpectedValue
            };

            var (success, errorMessage) = await _crmService.CreateLeadAsync(lead, userId, userName);
            if (!success)
            {
                return BadRequest(new { message = errorMessage });
            }

            dto.LeadId = lead.LeadId;
            dto.LeadCode = lead.LeadCode;
            dto.CreatedDate = lead.CreatedDate;

            return CreatedAtAction(nameof(GetLeads), new { id = lead.LeadId }, dto);
        }
    }
}
