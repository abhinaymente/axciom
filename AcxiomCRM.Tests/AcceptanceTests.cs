using AcxiomCRM.Data;
using AcxiomCRM.Models;
using AcxiomCRM.Services;
using AcxiomCRM.ViewModels;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AcxiomCRM.Tests
{
    public class AcceptanceTests
    {
        private ApplicationDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task Test1_CustomerEmailAndPhoneUniqueness_ServerSideRejection()
        {
            var context = GetInMemoryContext();
            var auditService = new AuditLogService(context);
            var crmService = new CrmService(context, auditService);

            // Create initial customer
            var customer1 = new Customer
            {
                CustomerName = "Original Corp",
                Email = "duplicate@test.com",
                Phone = "+1 555 111 2222"
            };
            var (success1, _) = await crmService.CreateCustomerAsync(customer1, "user1", "User One");
            Assert.True(success1);

            // Attempt duplicate Email
            var customer2 = new Customer
            {
                CustomerName = "Duplicate Email Corp",
                Email = "duplicate@test.com",
                Phone = "+1 555 999 8888"
            };
            var (success2, err2) = await crmService.CreateCustomerAsync(customer2, "user1", "User One");
            Assert.False(success2);
            Assert.Contains("already in use", err2);

            // Attempt duplicate Phone
            var customer3 = new Customer
            {
                CustomerName = "Duplicate Phone Corp",
                Email = "unique@test.com",
                Phone = "+1 555 111 2222"
            };
            var (success3, err3) = await crmService.CreateCustomerAsync(customer3, "user1", "User One");
            Assert.False(success3);
            Assert.Contains("already in use", err3);
        }

        [Fact]
        public async Task Test2_OpportunityBusinessRules_Validation()
        {
            var context = GetInMemoryContext();
            var auditService = new AuditLogService(context);
            var crmService = new CrmService(context, auditService);

            // 1. Amount <= 0 rejected
            var oppNegativeAmount = new Opportunity
            {
                OpportunityName = "Negative Deal",
                Amount = -5000m,
                Probability = 50,
                ExpectedCloseDate = DateTime.Today.AddDays(10),
                Status = "Active"
            };
            var (res1, err1) = await crmService.CreateOpportunityAsync(oppNegativeAmount, "user1", "User One");
            Assert.False(res1);
            Assert.Equal("Opportunity Amount must be greater than 0.", err1);

            // 2. Probability > 100 rejected
            var oppInvalidProb = new Opportunity
            {
                OpportunityName = "Over Prob Deal",
                Amount = 10000m,
                Probability = 150,
                ExpectedCloseDate = DateTime.Today.AddDays(10),
                Status = "Active"
            };
            var (res2, err2) = await crmService.CreateOpportunityAsync(oppInvalidProb, "user1", "User One");
            Assert.False(res2);
            Assert.Equal("Probability must be between 0 and 100.", err2);

            // 3. ExpectedCloseDate in past rejected for active opps
            var oppPastDate = new Opportunity
            {
                OpportunityName = "Past Date Deal",
                Amount = 10000m,
                Probability = 50,
                ExpectedCloseDate = DateTime.Today.AddDays(-5),
                Status = "Active"
            };
            var (res3, err3) = await crmService.CreateOpportunityAsync(oppPastDate, "user1", "User One");
            Assert.False(res3);
            Assert.Equal("Expected Close Date cannot be in the past.", err3);
        }

        [Fact]
        public async Task Test3_FollowUpDateBeforeToday_Rejected()
        {
            var context = GetInMemoryContext();
            var auditService = new AuditLogService(context);
            var crmService = new CrmService(context, auditService);

            var pastFollowUp = new FollowUp
            {
                FollowUpDate = DateTime.Today.AddDays(-1),
                FollowUpType = "Call",
                Status = "Planned"
            };

            var (res, err) = await crmService.CreateFollowUpAsync(pastFollowUp, "user1", "User One");
            Assert.False(res);
            Assert.Equal("Follow-up date cannot be earlier than today.", err);
        }

        [Fact]
        public async Task Test4_RoleBasedScoping_SalesExecutiveOnlySeesAssigned()
        {
            var context = GetInMemoryContext();
            var auditService = new AuditLogService(context);
            var crmService = new CrmService(context, auditService);

            // Seed customers assigned to userA and userB
            context.Customers.Add(new Customer { CustomerName = "User A Deal", Email = "a@a.com", Phone = "111", AssignedTo = "userA" });
            context.Customers.Add(new Customer { CustomerName = "User B Deal", Email = "b@b.com", Phone = "222", AssignedTo = "userB" });
            await context.SaveChangesAsync();

            // SalesExecutive query for userA
            var execResults = await crmService.GetCustomersAsync("userA", "SalesExecutive");
            Assert.Single(execResults);
            Assert.Equal("User A Deal", execResults[0].CustomerName);

            // Manager query sees all
            var mgrResults = await crmService.GetCustomersAsync("userA", "Manager");
            Assert.Equal(2, mgrResults.Count);
        }

        [Fact]
        public async Task Test5_AuditLogCreationOnCrudOperations()
        {
            var context = GetInMemoryContext();
            var auditService = new AuditLogService(context);
            var crmService = new CrmService(context, auditService);

            var cust = new Customer
            {
                CustomerName = "Audit Corp",
                Email = "audit@corp.com",
                Phone = "+1 999 888 777"
            };

            await crmService.CreateCustomerAsync(cust, "userAdmin", "Admin User");

            var logs = await auditService.GetAuditLogsAsync();
            Assert.NotEmpty(logs);
            Assert.Contains(logs, l => l.Action == "CREATE" && l.EntityName == "Customer" && l.NewValue == "Audit Corp");
        }

        [Fact]
        public async Task Test6_LeadConversionToCustomerAndOpportunity()
        {
            var context = GetInMemoryContext();
            var auditService = new AuditLogService(context);
            var crmService = new CrmService(context, auditService);

            var lead = new Lead
            {
                LeadName = "Tech Lead",
                Email = "techlead@test.com",
                Phone = "+1 800 555 1234",
                ExpectedValue = 50000m,
                Status = "Qualified"
            };
            await crmService.CreateLeadAsync(lead, "user1", "User One");

            var convertVm = new LeadConvertViewModel
            {
                LeadId = lead.LeadId,
                CreateCustomer = true,
                CustomerName = "Tech Lead Enterprise",
                CreateOpportunity = true,
                OpportunityName = "Tech Lead Enterprise Deal",
                OpportunityAmount = 50000m,
                Stage = "Proposal",
                Probability = 70,
                ExpectedCloseDate = DateTime.Today.AddDays(20)
            };

            var (success, _) = await crmService.ConvertLeadAsync(convertVm, "user1", "User One");
            Assert.True(success);

            var updatedLead = await crmService.GetLeadByIdAsync(lead.LeadId, "user1", "Admin");
            Assert.NotNull(updatedLead);
            Assert.Equal("Converted", updatedLead.Status);
            Assert.NotNull(updatedLead.ConvertedCustomerId);
            Assert.NotNull(updatedLead.ConvertedOpportunityId);
        }
    }
}
