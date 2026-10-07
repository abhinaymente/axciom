using AcxiomCRM.Models;
using AcxiomCRM.ViewModels;

namespace AcxiomCRM.Services
{
    public interface ICrmService
    {
        // Customers
        Task<List<Customer>> GetCustomersAsync(string userId, string role, string? search = null, string? status = null);
        Task<Customer?> GetCustomerByIdAsync(int id, string userId, string role);
        Task<(bool Success, string ErrorMessage)> CreateCustomerAsync(Customer customer, string userId, string userName);
        Task<(bool Success, string ErrorMessage)> UpdateCustomerAsync(Customer customer, string userId, string role, string userName);
        Task<(bool Success, string ErrorMessage)> DeleteCustomerAsync(int id, string userId, string role, string userName);

        // Leads
        Task<List<Lead>> GetLeadsAsync(string userId, string role, string? search = null, string? status = null);
        Task<Lead?> GetLeadByIdAsync(int id, string userId, string role);
        Task<(bool Success, string ErrorMessage)> CreateLeadAsync(Lead lead, string userId, string userName);
        Task<(bool Success, string ErrorMessage)> UpdateLeadAsync(Lead lead, string userId, string role, string userName);
        Task<(bool Success, string ErrorMessage)> DeleteLeadAsync(int id, string userId, string role, string userName);
        Task<(bool Success, string ErrorMessage)> ConvertLeadAsync(LeadConvertViewModel model, string userId, string userName);

        // Opportunities
        Task<List<Opportunity>> GetOpportunitiesAsync(string userId, string role, string? search = null, string? stage = null, string? status = null);
        Task<Opportunity?> GetOpportunityByIdAsync(int id, string userId, string role);
        Task<(bool Success, string ErrorMessage)> CreateOpportunityAsync(Opportunity opportunity, string userId, string userName);
        Task<(bool Success, string ErrorMessage)> UpdateOpportunityAsync(Opportunity opportunity, string userId, string role, string userName);
        Task<(bool Success, string ErrorMessage)> DeleteOpportunityAsync(int id, string userId, string role, string userName);

        // FollowUps
        Task<List<FollowUp>> GetFollowUpsAsync(string userId, string role, string? search = null, string? status = null, string? type = null);
        Task<FollowUp?> GetFollowUpByIdAsync(int id, string userId, string role);
        Task<(bool Success, string ErrorMessage)> CreateFollowUpAsync(FollowUp followUp, string userId, string userName);
        Task<(bool Success, string ErrorMessage)> UpdateFollowUpAsync(FollowUp followUp, string userId, string role, string userName);
        Task<(bool Success, string ErrorMessage)> DeleteFollowUpAsync(int id, string userId, string role, string userName);

        // Activities
        Task<List<Activity>> GetActivitiesAsync(string userId, string role, string? search = null, string? type = null);
        Task<Activity?> GetActivityByIdAsync(int id, string userId, string role);
        Task<(bool Success, string ErrorMessage)> CreateActivityAsync(Activity activity, string userId, string userName);

        // Dashboard
        Task<DashboardViewModel> GetDashboardDataAsync(string userId, string role);
    }
}
