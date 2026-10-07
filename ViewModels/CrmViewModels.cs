using System.ComponentModel.DataAnnotations;
using AcxiomCRM.Models;

namespace AcxiomCRM.ViewModels
{
    public class CustomerListViewModel
    {
        public List<Customer> Customers { get; set; } = new();
        public string? SearchTerm { get; set; }
        public string? StatusFilter { get; set; }
        public int PageNumber { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
    }

    public class LeadListViewModel
    {
        public List<Lead> Leads { get; set; } = new();
        public string? SearchTerm { get; set; }
        public string? StatusFilter { get; set; }
        public int PageNumber { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
    }

    public class LeadConvertViewModel
    {
        public int LeadId { get; set; }
        public string LeadName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? CompanyName { get; set; }

        public bool CreateCustomer { get; set; } = true;

        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; } = string.Empty;

        public bool CreateOpportunity { get; set; } = true;

        [Display(Name = "Opportunity Name")]
        public string OpportunityName { get; set; } = string.Empty;

        [Display(Name = "Opportunity Amount")]
        [Range(0.01, 1000000000.00, ErrorMessage = "Opportunity Amount must be greater than 0.")]
        public decimal OpportunityAmount { get; set; }

        [Display(Name = "Stage")]
        public string Stage { get; set; } = "Qualification";

        [Display(Name = "Probability (%)")]
        [Range(0, 100, ErrorMessage = "Probability must be between 0 and 100.")]
        public int Probability { get; set; } = 50;

        [DataType(DataType.Date)]
        [Display(Name = "Expected Close Date")]
        public DateTime ExpectedCloseDate { get; set; } = DateTime.Today.AddDays(30);
    }

    public class OpportunityListViewModel
    {
        public List<Opportunity> Opportunities { get; set; } = new();
        public string? SearchTerm { get; set; }
        public string? StageFilter { get; set; }
        public string? StatusFilter { get; set; }
        public decimal TotalPipelineValue { get; set; }
        public int PageNumber { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
    }

    public class FollowUpListViewModel
    {
        public List<FollowUp> FollowUps { get; set; } = new();
        public string? SearchTerm { get; set; }
        public string? StatusFilter { get; set; }
        public string? TypeFilter { get; set; }
        public int PageNumber { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
    }

    public class ActivityListViewModel
    {
        public List<Activity> Activities { get; set; } = new();
        public string? SearchTerm { get; set; }
        public string? TypeFilter { get; set; }
        public int PageNumber { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
    }

    public class DashboardViewModel
    {
        public int TotalCustomers { get; set; }
        public int TotalLeads { get; set; }
        public int OpenLeads { get; set; }
        public int TotalOpportunities { get; set; }
        public int OpenOpportunities { get; set; }
        public int WonOpportunities { get; set; }
        public int LostOpportunities { get; set; }
        public decimal TotalPipelineValue { get; set; }

        // Chart Data
        public Dictionary<string, int> LeadStatusCounts { get; set; } = new();
        public Dictionary<string, int> OpportunityStageCounts { get; set; } = new();
        public Dictionary<string, decimal> MonthlySalesData { get; set; } = new();

        public List<FollowUp> UpcomingFollowUps { get; set; } = new();
        public List<Opportunity> RecentOpportunities { get; set; } = new();
    }

    public class ReportFilterViewModel
    {
        public string ReportType { get; set; } = "Customer"; // Customer, Lead, FollowUp, Opportunity, Pipeline, Conversion, UserActivity, Audit
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? StatusFilter { get; set; }
        public string? SearchTerm { get; set; }
    }
}
