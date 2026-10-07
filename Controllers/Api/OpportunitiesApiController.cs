using AcxiomCRM.Models;
using AcxiomCRM.Models.DTOs;
using AcxiomCRM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AcxiomCRM.Controllers.Api
{
    [Route("api/opportunities")]
    [ApiController]
    [Authorize]
    public class OpportunitiesApiController : ControllerBase
    {
        private readonly ICrmService _crmService;
        private readonly UserManager<ApplicationUser> _userManager;

        public OpportunitiesApiController(ICrmService crmService, UserManager<ApplicationUser> userManager)
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
        public async Task<ActionResult<IEnumerable<OpportunityDto>>> GetOpportunities([FromQuery] string? search, [FromQuery] string? stage)
        {
            var (userId, role, _) = await GetCurrentUserInfoAsync();
            var opps = await _crmService.GetOpportunitiesAsync(userId, role, search, stage);

            var dtos = opps.Select(o => new OpportunityDto
            {
                OpportunityId = o.OpportunityId,
                OpportunityName = o.OpportunityName,
                CustomerId = o.CustomerId,
                LeadId = o.LeadId,
                Amount = o.Amount,
                Stage = o.Stage,
                Probability = o.Probability,
                ExpectedCloseDate = o.ExpectedCloseDate,
                Status = o.Status,
                CreatedDate = o.CreatedDate
            });

            return Ok(dtos);
        }

        [HttpPost]
        public async Task<ActionResult<OpportunityDto>> CreateOpportunity([FromBody] OpportunityDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (userId, role, userName) = await GetCurrentUserInfoAsync();

            var opp = new Opportunity
            {
                OpportunityName = dto.OpportunityName,
                CustomerId = dto.CustomerId,
                LeadId = dto.LeadId,
                Amount = dto.Amount,
                Stage = string.IsNullOrWhiteSpace(dto.Stage) ? "Qualification" : dto.Stage,
                Probability = dto.Probability,
                ExpectedCloseDate = dto.ExpectedCloseDate == default ? DateTime.Today.AddDays(30) : dto.ExpectedCloseDate,
                Status = string.IsNullOrWhiteSpace(dto.Status) ? "Active" : dto.Status
            };

            var (success, errorMessage) = await _crmService.CreateOpportunityAsync(opp, userId, userName);
            if (!success)
            {
                return BadRequest(new { message = errorMessage });
            }

            dto.OpportunityId = opp.OpportunityId;
            dto.CreatedDate = opp.CreatedDate;

            return CreatedAtAction(nameof(GetOpportunities), new { id = opp.OpportunityId }, dto);
        }
    }
}
