using AcxiomCRM.Data;
using AcxiomCRM.Models;
using AcxiomCRM.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Services
{
    public class CrmService : ICrmService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditLogService _auditLogService;

        public CrmService(ApplicationDbContext context, IAuditLogService auditLogService)
        {
            _context = context;
            _auditLogService = auditLogService;
        }

        #region Helper Scoping
        private bool IsExecutive(string role) => role.Equals("SalesExecutive", StringComparison.OrdinalIgnoreCase);
        #endregion

        #region Customers
        public async Task<List<Customer>> GetCustomersAsync(string userId, string role, string? search = null, string? status = null)
        {
            var query = _context.Customers.AsQueryable();

            if (IsExecutive(role))
            {
                query = query.Where(c => c.AssignedTo == userId || c.CreatedBy == userId);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c => c.CustomerName.Contains(search) ||
                                         c.Email.Contains(search) ||
                                         c.Phone.Contains(search) ||
                                         (c.CompanyName != null && c.CompanyName.Contains(search)));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(c => c.Status == status);
            }

            return await query.OrderByDescending(c => c.CreatedDate).ToListAsync();
        }

        public async Task<Customer?> GetCustomerByIdAsync(int id, string userId, string role)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerId == id);
            if (customer == null) return null;

            if (IsExecutive(role) && customer.AssignedTo != userId && customer.CreatedBy != userId)
            {
                return null; // Unauthorized scope
            }

            return customer;
        }

        public async Task<(bool Success, string ErrorMessage)> CreateCustomerAsync(Customer customer, string userId, string userName)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(customer.CustomerName))
                return (false, "Enter a valid customer name.");
            if (string.IsNullOrWhiteSpace(customer.Email))
                return (false, "Enter a valid email address.");
            if (string.IsNullOrWhiteSpace(customer.Phone))
                return (false, "Enter a valid phone number.");

            // Email uniqueness
            if (await _context.Customers.AnyAsync(c => c.Email == customer.Email))
                return (false, "Email address is already in use by another customer.");

            // Phone uniqueness
            if (await _context.Customers.AnyAsync(c => c.Phone == customer.Phone))
                return (false, "Phone number is already in use by another customer.");

            if (string.IsNullOrWhiteSpace(customer.CustomerCode))
            {
                customer.CustomerCode = $"CUST-{Random.Shared.Next(1000, 9999)}";
            }

            customer.CreatedDate = DateTime.UtcNow;
            customer.CreatedBy = userId;
            if (string.IsNullOrWhiteSpace(customer.AssignedTo))
            {
                customer.AssignedTo = userId;
            }

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(userId, userName, "CREATE", "Customer", customer.CustomerId.ToString(), null, customer.CustomerName);

            return (true, string.Empty);
        }

        public async Task<(bool Success, string ErrorMessage)> UpdateCustomerAsync(Customer customer, string userId, string role, string userName)
        {
            var existing = await GetCustomerByIdAsync(customer.CustomerId, userId, role);
            if (existing == null)
                return (false, "Customer record not found or access denied.");

            if (string.IsNullOrWhiteSpace(customer.CustomerName))
                return (false, "Enter a valid customer name.");
            if (string.IsNullOrWhiteSpace(customer.Email))
                return (false, "Enter a valid email address.");
            if (string.IsNullOrWhiteSpace(customer.Phone))
                return (false, "Enter a valid phone number.");

            // Email uniqueness
            if (await _context.Customers.AnyAsync(c => c.Email == customer.Email && c.CustomerId != customer.CustomerId))
                return (false, "Email address is already in use by another customer.");

            // Phone uniqueness
            if (await _context.Customers.AnyAsync(c => c.Phone == customer.Phone && c.CustomerId != customer.CustomerId))
                return (false, "Phone number is already in use by another customer.");

            string oldValue = $"{existing.CustomerName} ({existing.Email})";

            existing.CustomerName = customer.CustomerName;
            existing.Email = customer.Email;
            existing.Phone = customer.Phone;
            existing.CompanyName = customer.CompanyName;
            existing.Address = customer.Address;
            existing.City = customer.City;
            existing.State = customer.State;
            existing.Status = customer.Status;
            if (!string.IsNullOrWhiteSpace(customer.AssignedTo))
            {
                existing.AssignedTo = customer.AssignedTo;
            }

            await _context.SaveChangesAsync();

            string newValue = $"{existing.CustomerName} ({existing.Email})";
            await _auditLogService.LogAsync(userId, userName, "UPDATE", "Customer", existing.CustomerId.ToString(), oldValue, newValue);

            return (true, string.Empty);
        }

        public async Task<(bool Success, string ErrorMessage)> DeleteCustomerAsync(int id, string userId, string role, string userName)
        {
            var customer = await GetCustomerByIdAsync(id, userId, role);
            if (customer == null)
                return (false, "Customer not found or access denied.");

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(userId, userName, "DELETE", "Customer", id.ToString(), customer.CustomerName, null);
            return (true, string.Empty);
        }
        #endregion

        #region Leads
        public async Task<List<Lead>> GetLeadsAsync(string userId, string role, string? search = null, string? status = null)
        {
            var query = _context.Leads
                .Include(l => l.ConvertedCustomer)
                .Include(l => l.ConvertedOpportunity)
                .AsQueryable();

            if (IsExecutive(role))
            {
                query = query.Where(l => l.AssignedTo == userId);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(l => l.LeadName.Contains(search) ||
                                         l.Email.Contains(search) ||
                                         l.Phone.Contains(search) ||
                                         (l.CompanyName != null && l.CompanyName.Contains(search)));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(l => l.Status == status);
            }

            return await query.OrderByDescending(l => l.CreatedDate).ToListAsync();
        }

        public async Task<Lead?> GetLeadByIdAsync(int id, string userId, string role)
        {
            var lead = await _context.Leads
                .Include(l => l.ConvertedCustomer)
                .Include(l => l.ConvertedOpportunity)
                .FirstOrDefaultAsync(l => l.LeadId == id);

            if (lead == null) return null;

            if (IsExecutive(role) && lead.AssignedTo != userId)
            {
                return null;
            }

            return lead;
        }

        public async Task<(bool Success, string ErrorMessage)> CreateLeadAsync(Lead lead, string userId, string userName)
        {
            if (string.IsNullOrWhiteSpace(lead.LeadName))
                return (false, "Enter a valid lead name.");
            if (string.IsNullOrWhiteSpace(lead.Email))
                return (false, "Enter a valid email address.");
            if (string.IsNullOrWhiteSpace(lead.Phone))
                return (false, "Enter a valid phone number.");
            if (lead.ExpectedValue < 0)
                return (false, "Expected Value cannot be negative.");

            if (string.IsNullOrWhiteSpace(lead.LeadCode))
            {
                lead.LeadCode = $"LEAD-{Random.Shared.Next(1000, 9999)}";
            }

            lead.CreatedDate = DateTime.UtcNow;
            if (string.IsNullOrWhiteSpace(lead.AssignedTo))
            {
                lead.AssignedTo = userId;
            }

            _context.Leads.Add(lead);
            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(userId, userName, "CREATE", "Lead", lead.LeadId.ToString(), null, lead.LeadName);
            return (true, string.Empty);
        }

        public async Task<(bool Success, string ErrorMessage)> UpdateLeadAsync(Lead lead, string userId, string role, string userName)
        {
            var existing = await GetLeadByIdAsync(lead.LeadId, userId, role);
            if (existing == null)
                return (false, "Lead record not found or access denied.");

            if (string.IsNullOrWhiteSpace(lead.LeadName))
                return (false, "Enter a valid lead name.");
            if (string.IsNullOrWhiteSpace(lead.Email))
                return (false, "Enter a valid email address.");
            if (string.IsNullOrWhiteSpace(lead.Phone))
                return (false, "Enter a valid phone number.");
            if (lead.ExpectedValue < 0)
                return (false, "Expected Value cannot be negative.");

            string oldValue = $"{existing.LeadName} ({existing.Status})";

            existing.LeadName = lead.LeadName;
            existing.Email = lead.Email;
            existing.Phone = lead.Phone;
            existing.CompanyName = lead.CompanyName;
            existing.Source = lead.Source;
            existing.Status = lead.Status;
            existing.ExpectedValue = lead.ExpectedValue;
            if (!string.IsNullOrWhiteSpace(lead.AssignedTo))
            {
                existing.AssignedTo = lead.AssignedTo;
            }

            await _context.SaveChangesAsync();

            string newValue = $"{existing.LeadName} ({existing.Status})";
            await _auditLogService.LogAsync(userId, userName, "UPDATE", "Lead", existing.LeadId.ToString(), oldValue, newValue);
            return (true, string.Empty);
        }

        public async Task<(bool Success, string ErrorMessage)> DeleteLeadAsync(int id, string userId, string role, string userName)
        {
            var lead = await GetLeadByIdAsync(id, userId, role);
            if (lead == null)
                return (false, "Lead not found or access denied.");

            _context.Leads.Remove(lead);
            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(userId, userName, "DELETE", "Lead", id.ToString(), lead.LeadName, null);
            return (true, string.Empty);
        }

        public async Task<(bool Success, string ErrorMessage)> ConvertLeadAsync(LeadConvertViewModel model, string userId, string userName)
        {
            var lead = await _context.Leads.FirstOrDefaultAsync(l => l.LeadId == model.LeadId);
            if (lead == null)
                return (false, "Lead not found.");

            if (lead.Status == "Converted")
                return (false, "Lead has already been converted.");

            Customer? newCustomer = null;
            Opportunity? newOpp = null;

            if (model.CreateCustomer)
            {
                string custName = string.IsNullOrWhiteSpace(model.CustomerName) ? lead.LeadName : model.CustomerName;
                newCustomer = new Customer
                {
                    CustomerCode = $"CUST-{Random.Shared.Next(1000, 9999)}",
                    CustomerName = custName,
                    Email = lead.Email,
                    Phone = lead.Phone,
                    CompanyName = lead.CompanyName,
                    Status = "Active",
                    CreatedBy = userId,
                    AssignedTo = lead.AssignedTo,
                    CreatedDate = DateTime.UtcNow
                };

                // Check uniqueness for created customer
                if (!await _context.Customers.AnyAsync(c => c.Email == newCustomer.Email))
                {
                    _context.Customers.Add(newCustomer);
                    await _context.SaveChangesAsync();
                    lead.ConvertedCustomerId = newCustomer.CustomerId;
                }
                else
                {
                    var existingCust = await _context.Customers.FirstOrDefaultAsync(c => c.Email == newCustomer.Email);
                    if (existingCust != null)
                    {
                        lead.ConvertedCustomerId = existingCust.CustomerId;
                        newCustomer = existingCust;
                    }
                }
            }

            if (model.CreateOpportunity)
            {
                if (model.OpportunityAmount <= 0)
                    return (false, "Opportunity Amount must be greater than 0.");
                if (model.Probability < 0 || model.Probability > 100)
                    return (false, "Probability must be between 0 and 100.");
                if (model.ExpectedCloseDate.Date < DateTime.Today)
                    return (false, "Expected Close Date cannot be in the past.");

                string oppName = string.IsNullOrWhiteSpace(model.OpportunityName) ? $"{lead.LeadName} Deal" : model.OpportunityName;
                newOpp = new Opportunity
                {
                    OpportunityName = oppName,
                    CustomerId = newCustomer?.CustomerId,
                    LeadId = lead.LeadId,
                    Amount = model.OpportunityAmount,
                    Stage = model.Stage,
                    Probability = model.Probability,
                    ExpectedCloseDate = model.ExpectedCloseDate,
                    Status = "Active",
                    AssignedTo = lead.AssignedTo,
                    CreatedDate = DateTime.UtcNow
                };

                _context.Opportunities.Add(newOpp);
                await _context.SaveChangesAsync();
                lead.ConvertedOpportunityId = newOpp.OpportunityId;
            }

            lead.Status = "Converted";
            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(userId, userName, "CONVERT", "Lead", lead.LeadId.ToString(), "Qualified", "Converted");
            return (true, string.Empty);
        }
        #endregion

        #region Opportunities
        public async Task<List<Opportunity>> GetOpportunitiesAsync(string userId, string role, string? search = null, string? stage = null, string? status = null)
        {
            var query = _context.Opportunities
                .Include(o => o.Customer)
                .Include(o => o.Lead)
                .AsQueryable();

            if (IsExecutive(role))
            {
                query = query.Where(o => o.AssignedTo == userId);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(o => o.OpportunityName.Contains(search) ||
                                         (o.Customer != null && o.Customer.CustomerName.Contains(search)));
            }

            if (!string.IsNullOrWhiteSpace(stage))
            {
                query = query.Where(o => o.Stage == stage);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(o => o.Status == status);
            }

            return await query.OrderByDescending(o => o.CreatedDate).ToListAsync();
        }

        public async Task<Opportunity?> GetOpportunityByIdAsync(int id, string userId, string role)
        {
            var opp = await _context.Opportunities
                .Include(o => o.Customer)
                .Include(o => o.Lead)
                .FirstOrDefaultAsync(o => o.OpportunityId == id);

            if (opp == null) return null;

            if (IsExecutive(role) && opp.AssignedTo != userId)
            {
                return null;
            }

            return opp;
        }

        public async Task<(bool Success, string ErrorMessage)> CreateOpportunityAsync(Opportunity opportunity, string userId, string userName)
        {
            if (string.IsNullOrWhiteSpace(opportunity.OpportunityName))
                return (false, "Opportunity Name is required.");

            // Mandatory Business Rules:
            // 1. Amount cannot be negative and Active opportunity Amount must be > 0
            if (opportunity.Amount <= 0)
                return (false, "Opportunity Amount must be greater than 0.");

            // 2. Probability must be 0–100
            if (opportunity.Probability < 0 || opportunity.Probability > 100)
                return (false, "Probability must be between 0 and 100.");

            // 3. ExpectedCloseDate cannot be in the past for active opportunities
            if (opportunity.Status == "Active" && opportunity.ExpectedCloseDate.Date < DateTime.Today)
                return (false, "Expected Close Date cannot be in the past.");

            opportunity.CreatedDate = DateTime.UtcNow;
            if (string.IsNullOrWhiteSpace(opportunity.AssignedTo))
            {
                opportunity.AssignedTo = userId;
            }

            // Sync Status based on Stage
            if (opportunity.Stage == "Won") opportunity.Status = "Won";
            else if (opportunity.Stage == "Lost") opportunity.Status = "Lost";

            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(userId, userName, "CREATE", "Opportunity", opportunity.OpportunityId.ToString(), null, opportunity.OpportunityName);
            return (true, string.Empty);
        }

        public async Task<(bool Success, string ErrorMessage)> UpdateOpportunityAsync(Opportunity opportunity, string userId, string role, string userName)
        {
            var existing = await GetOpportunityByIdAsync(opportunity.OpportunityId, userId, role);
            if (existing == null)
                return (false, "Opportunity not found or access denied.");

            if (string.IsNullOrWhiteSpace(opportunity.OpportunityName))
                return (false, "Opportunity Name is required.");

            // Mandatory Business Rules
            if (opportunity.Amount <= 0)
                return (false, "Opportunity Amount must be greater than 0.");

            if (opportunity.Probability < 0 || opportunity.Probability > 100)
                return (false, "Probability must be between 0 and 100.");

            if (opportunity.Status == "Active" && opportunity.ExpectedCloseDate.Date < DateTime.Today)
                return (false, "Expected Close Date cannot be in the past.");

            string oldValue = $"{existing.OpportunityName} - {existing.Stage} - ${existing.Amount}";

            existing.OpportunityName = opportunity.OpportunityName;
            existing.CustomerId = opportunity.CustomerId;
            existing.LeadId = opportunity.LeadId;
            existing.Amount = opportunity.Amount;
            existing.Stage = opportunity.Stage;
            existing.Probability = opportunity.Probability;
            existing.ExpectedCloseDate = opportunity.ExpectedCloseDate;
            existing.Status = opportunity.Status;
            if (opportunity.Stage == "Won") existing.Status = "Won";
            else if (opportunity.Stage == "Lost") existing.Status = "Lost";

            if (!string.IsNullOrWhiteSpace(opportunity.AssignedTo))
            {
                existing.AssignedTo = opportunity.AssignedTo;
            }

            await _context.SaveChangesAsync();

            string newValue = $"{existing.OpportunityName} - {existing.Stage} - ${existing.Amount}";
            await _auditLogService.LogAsync(userId, userName, "UPDATE", "Opportunity", existing.OpportunityId.ToString(), oldValue, newValue);
            return (true, string.Empty);
        }

        public async Task<(bool Success, string ErrorMessage)> DeleteOpportunityAsync(int id, string userId, string role, string userName)
        {
            var opp = await GetOpportunityByIdAsync(id, userId, role);
            if (opp == null)
                return (false, "Opportunity not found or access denied.");

            _context.Opportunities.Remove(opp);
            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(userId, userName, "DELETE", "Opportunity", id.ToString(), opp.OpportunityName, null);
            return (true, string.Empty);
        }
        #endregion

        #region FollowUps
        public async Task<List<FollowUp>> GetFollowUpsAsync(string userId, string role, string? search = null, string? status = null, string? type = null)
        {
            var query = _context.FollowUps
                .Include(f => f.Customer)
                .Include(f => f.Lead)
                .AsQueryable();

            if (IsExecutive(role))
            {
                query = query.Where(f => f.AssignedTo == userId);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(f => (f.Customer != null && f.Customer.CustomerName.Contains(search)) ||
                                         (f.Lead != null && f.Lead.LeadName.Contains(search)) ||
                                         (f.Remarks != null && f.Remarks.Contains(search)));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(f => f.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(type))
            {
                query = query.Where(f => f.FollowUpType == type);
            }

            return await query.OrderBy(f => f.FollowUpDate).ToListAsync();
        }

        public async Task<FollowUp?> GetFollowUpByIdAsync(int id, string userId, string role)
        {
            var followUp = await _context.FollowUps
                .Include(f => f.Customer)
                .Include(f => f.Lead)
                .FirstOrDefaultAsync(f => f.FollowUpId == id);

            if (followUp == null) return null;

            if (IsExecutive(role) && followUp.AssignedTo != userId)
            {
                return null;
            }

            return followUp;
        }

        public async Task<(bool Success, string ErrorMessage)> CreateFollowUpAsync(FollowUp followUp, string userId, string userName)
        {
            // MANDATORY RULE: A new/planned follow-up cannot have a date earlier than today.
            if ((followUp.Status == "Planned" || string.IsNullOrWhiteSpace(followUp.Status)) && followUp.FollowUpDate.Date < DateTime.Today)
            {
                return (false, "Follow-up date cannot be earlier than today.");
            }

            followUp.CreatedDate = DateTime.UtcNow;
            if (string.IsNullOrWhiteSpace(followUp.AssignedTo))
            {
                followUp.AssignedTo = userId;
            }

            _context.FollowUps.Add(followUp);
            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(userId, userName, "CREATE", "FollowUp", followUp.FollowUpId.ToString(), null, $"{followUp.FollowUpType} on {followUp.FollowUpDate:d}");
            return (true, string.Empty);
        }

        public async Task<(bool Success, string ErrorMessage)> UpdateFollowUpAsync(FollowUp followUp, string userId, string role, string userName)
        {
            var existing = await GetFollowUpByIdAsync(followUp.FollowUpId, userId, role);
            if (existing == null)
                return (false, "Follow-up record not found or access denied.");

            if (followUp.Status == "Planned" && followUp.FollowUpDate.Date < DateTime.Today)
            {
                return (false, "Follow-up date cannot be earlier than today.");
            }

            existing.CustomerId = followUp.CustomerId;
            existing.LeadId = followUp.LeadId;
            existing.FollowUpDate = followUp.FollowUpDate;
            existing.FollowUpType = followUp.FollowUpType;
            existing.Remarks = followUp.Remarks;
            existing.Status = followUp.Status;
            if (!string.IsNullOrWhiteSpace(followUp.AssignedTo))
            {
                existing.AssignedTo = followUp.AssignedTo;
            }

            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(userId, userName, "UPDATE", "FollowUp", existing.FollowUpId.ToString(), null, existing.Status);
            return (true, string.Empty);
        }

        public async Task<(bool Success, string ErrorMessage)> DeleteFollowUpAsync(int id, string userId, string role, string userName)
        {
            var followUp = await GetFollowUpByIdAsync(id, userId, role);
            if (followUp == null)
                return (false, "Follow-up not found or access denied.");

            _context.FollowUps.Remove(followUp);
            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(userId, userName, "DELETE", "FollowUp", id.ToString(), null, null);
            return (true, string.Empty);
        }
        #endregion

        #region Activities
        public async Task<List<Activity>> GetActivitiesAsync(string userId, string role, string? search = null, string? type = null)
        {
            var query = _context.Activities
                .Include(a => a.Customer)
                .Include(a => a.Lead)
                .AsQueryable();

            if (IsExecutive(role))
            {
                query = query.Where(a => a.AssignedTo == userId);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(a => a.Subject.Contains(search) || (a.Description != null && a.Description.Contains(search)));
            }

            if (!string.IsNullOrWhiteSpace(type))
            {
                query = query.Where(a => a.ActivityType == type);
            }

            return await query.OrderByDescending(a => a.ActivityDate).ToListAsync();
        }

        public async Task<Activity?> GetActivityByIdAsync(int id, string userId, string role)
        {
            var act = await _context.Activities
                .Include(a => a.Customer)
                .Include(a => a.Lead)
                .FirstOrDefaultAsync(a => a.ActivityId == id);

            if (act == null) return null;

            if (IsExecutive(role) && act.AssignedTo != userId)
            {
                return null;
            }

            return act;
        }

        public async Task<(bool Success, string ErrorMessage)> CreateActivityAsync(Activity activity, string userId, string userName)
        {
            if (string.IsNullOrWhiteSpace(activity.Subject))
                return (false, "Subject is required.");

            if (string.IsNullOrWhiteSpace(activity.AssignedTo))
            {
                activity.AssignedTo = userId;
            }

            _context.Activities.Add(activity);
            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(userId, userName, "CREATE", "Activity", activity.ActivityId.ToString(), null, activity.Subject);
            return (true, string.Empty);
        }
        #endregion

        #region Dashboard
        public async Task<DashboardViewModel> GetDashboardDataAsync(string userId, string role)
        {
            var customers = await GetCustomersAsync(userId, role);
            var leads = await GetLeadsAsync(userId, role);
            var opps = await GetOpportunitiesAsync(userId, role);
            var followUps = await GetFollowUpsAsync(userId, role);

            var vm = new DashboardViewModel
            {
                TotalCustomers = customers.Count,
                TotalLeads = leads.Count,
                OpenLeads = leads.Count(l => l.Status != "Lost" && l.Status != "Converted"),
                TotalOpportunities = opps.Count,
                OpenOpportunities = opps.Count(o => o.Status == "Active" || o.Stage == "Qualification" || o.Stage == "Proposal" || o.Stage == "Negotiation"),
                WonOpportunities = opps.Count(o => o.Status == "Won" || o.Stage == "Won"),
                LostOpportunities = opps.Count(o => o.Status == "Lost" || o.Stage == "Lost"),
                TotalPipelineValue = opps.Where(o => o.Status == "Active" || o.Stage == "Qualification" || o.Stage == "Proposal" || o.Stage == "Negotiation").Sum(o => o.Amount),
                UpcomingFollowUps = followUps.Where(f => f.Status == "Planned" && f.FollowUpDate >= DateTime.Today).Take(5).ToList(),
                RecentOpportunities = opps.OrderByDescending(o => o.CreatedDate).Take(5).ToList()
            };

            // Lead Status Breakdown
            vm.LeadStatusCounts = new Dictionary<string, int>
            {
                { "New", leads.Count(l => l.Status == "New") },
                { "Contacted", leads.Count(l => l.Status == "Contacted") },
                { "Qualified", leads.Count(l => l.Status == "Qualified") },
                { "Lost", leads.Count(l => l.Status == "Lost") },
                { "Converted", leads.Count(l => l.Status == "Converted") }
            };

            // Opportunity Pipeline Breakdown
            vm.OpportunityStageCounts = new Dictionary<string, int>
            {
                { "Qualification", opps.Count(o => o.Stage == "Qualification") },
                { "Proposal", opps.Count(o => o.Stage == "Proposal") },
                { "Negotiation", opps.Count(o => o.Stage == "Negotiation") },
                { "Won", opps.Count(o => o.Stage == "Won") },
                { "Lost", opps.Count(o => o.Stage == "Lost") }
            };

            // Monthly Sales Data (Last 6 Months)
            var monthlyData = new Dictionary<string, decimal>();
            for (int i = 5; i >= 0; i--)
            {
                var monthDate = DateTime.Today.AddMonths(-i);
                string monthLabel = monthDate.ToString("MMM yyyy");
                decimal monthWonSum = opps.Where(o => (o.Stage == "Won" || o.Status == "Won") &&
                                                       o.CreatedDate.Month == monthDate.Month &&
                                                       o.CreatedDate.Year == monthDate.Year)
                                          .Sum(o => o.Amount);
                monthlyData[monthLabel] = monthWonSum;
            }
            vm.MonthlySalesData = monthlyData;

            return vm;
        }
        #endregion
    }
}
