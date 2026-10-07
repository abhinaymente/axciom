using AcxiomCRM.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Ensure DB created
            await context.Database.EnsureCreatedAsync();

            // 1. Seed Roles
            string[] roles = new[] { "Admin", "Manager", "SalesExecutive" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. Seed Users
            const string demoPassword = "AcxiomPass123!";

            // Admin
            var adminUser = await userManager.FindByEmailAsync("admin@acxiomcrm.com");
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = "admin@acxiomcrm.com",
                    Email = "admin@acxiomcrm.com",
                    FullName = "System Administrator",
                    Department = "IT / Security",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };
                var result = await userManager.CreateAsync(adminUser, demoPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Manager
            var managerUser = await userManager.FindByEmailAsync("manager@acxiomcrm.com");
            if (managerUser == null)
            {
                managerUser = new ApplicationUser
                {
                    UserName = "manager@acxiomcrm.com",
                    Email = "manager@acxiomcrm.com",
                    FullName = "Sales Manager",
                    Department = "Sales Management",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };
                var result = await userManager.CreateAsync(managerUser, demoPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(managerUser, "Manager");
                }
            }

            // Sales Executive
            var salesUser = await userManager.FindByEmailAsync("sales@acxiomcrm.com");
            if (salesUser == null)
            {
                salesUser = new ApplicationUser
                {
                    UserName = "sales@acxiomcrm.com",
                    Email = "sales@acxiomcrm.com",
                    FullName = "John Executive",
                    Department = "Field Sales",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };
                var result = await userManager.CreateAsync(salesUser, demoPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(salesUser, "SalesExecutive");
                }
            }

            // 3. Seed Realistic Demo CRM Data if empty
            if (!await context.Customers.AnyAsync())
            {
                var customers = new List<Customer>
                {
                    new Customer
                    {
                        CustomerCode = "CUST-1001",
                        CustomerName = "Acme Global Solutions",
                        Email = "contact@acmeglobal.com",
                        Phone = "+1 555 019 2831",
                        CompanyName = "Acme Corp",
                        Address = "100 Tech Parkway",
                        City = "San Jose",
                        State = "CA",
                        Status = "Active",
                        CreatedBy = adminUser.Id,
                        AssignedTo = salesUser.Id,
                        CreatedDate = DateTime.UtcNow.AddDays(-30)
                    },
                    new Customer
                    {
                        CustomerCode = "CUST-1002",
                        CustomerName = "Apex Financial Group",
                        Email = "info@apexfinancial.com",
                        Phone = "+1 555 014 9920",
                        CompanyName = "Apex Financial",
                        Address = "500 Wall Street",
                        City = "New York",
                        State = "NY",
                        Status = "Active",
                        CreatedBy = adminUser.Id,
                        AssignedTo = salesUser.Id,
                        CreatedDate = DateTime.UtcNow.AddDays(-20)
                    },
                    new Customer
                    {
                        CustomerCode = "CUST-1003",
                        CustomerName = "Starlight Retail Inc",
                        Email = "purchasing@starlight.com",
                        Phone = "+1 555 018 3344",
                        CompanyName = "Starlight Retail",
                        Address = "75 Market Street",
                        City = "Chicago",
                        State = "IL",
                        Status = "Active",
                        CreatedBy = managerUser.Id,
                        AssignedTo = managerUser.Id,
                        CreatedDate = DateTime.UtcNow.AddDays(-10)
                    }
                };

                await context.Customers.AddRangeAsync(customers);
                await context.SaveChangesAsync();

                // Seed Leads
                var leads = new List<Lead>
                {
                    new Lead
                    {
                        LeadCode = "LEAD-2001",
                        LeadName = "Nexus Logistics Software Upgrade",
                        Email = "sales@nexuslogistics.com",
                        Phone = "+1 555 012 8871",
                        CompanyName = "Nexus Logistics",
                        Source = "Website",
                        Status = "New",
                        ExpectedValue = 45000m,
                        AssignedTo = salesUser.Id,
                        CreatedDate = DateTime.UtcNow.AddDays(-5)
                    },
                    new Lead
                    {
                        LeadCode = "LEAD-2002",
                        LeadName = "Quantum BioTech CRM Inquiry",
                        Email = "lead@quantumbio.com",
                        Phone = "+1 555 013 7762",
                        CompanyName = "Quantum BioTech",
                        Source = "Referral",
                        Status = "Contacted",
                        ExpectedValue = 85000m,
                        AssignedTo = salesUser.Id,
                        CreatedDate = DateTime.UtcNow.AddDays(-8)
                    },
                    new Lead
                    {
                        LeadCode = "LEAD-2003",
                        LeadName = "Vanguard Tech Hardware Deal",
                        Email = "tech@vanguard.com",
                        Phone = "+1 555 017 4423",
                        CompanyName = "Vanguard Systems",
                        Source = "Trade Show",
                        Status = "Qualified",
                        ExpectedValue = 120000m,
                        AssignedTo = managerUser.Id,
                        CreatedDate = DateTime.UtcNow.AddDays(-12)
                    }
                };

                await context.Leads.AddRangeAsync(leads);
                await context.SaveChangesAsync();

                // Seed Opportunities
                var opps = new List<Opportunity>
                {
                    new Opportunity
                    {
                        OpportunityName = "Acme ERP Expansion Contract",
                        CustomerId = customers[0].CustomerId,
                        Amount = 150000m,
                        Stage = "Proposal",
                        Probability = 60,
                        ExpectedCloseDate = DateTime.Today.AddDays(15),
                        Status = "Active",
                        AssignedTo = salesUser.Id,
                        CreatedDate = DateTime.UtcNow.AddDays(-15)
                    },
                    new Opportunity
                    {
                        OpportunityName = "Apex Cloud Migration Project",
                        CustomerId = customers[1].CustomerId,
                        Amount = 280000m,
                        Stage = "Negotiation",
                        Probability = 80,
                        ExpectedCloseDate = DateTime.Today.AddDays(25),
                        Status = "Active",
                        AssignedTo = salesUser.Id,
                        CreatedDate = DateTime.UtcNow.AddDays(-20)
                    },
                    new Opportunity
                    {
                        OpportunityName = "Starlight POS System Rollout",
                        CustomerId = customers[2].CustomerId,
                        Amount = 95000m,
                        Stage = "Won",
                        Probability = 100,
                        ExpectedCloseDate = DateTime.Today.AddDays(-2),
                        Status = "Won",
                        AssignedTo = managerUser.Id,
                        CreatedDate = DateTime.UtcNow.AddDays(-40)
                    },
                    new Opportunity
                    {
                        OpportunityName = "Legacy Security Audit Deal",
                        CustomerId = customers[0].CustomerId,
                        Amount = 35000m,
                        Stage = "Lost",
                        Probability = 0,
                        ExpectedCloseDate = DateTime.Today.AddDays(-10),
                        Status = "Lost",
                        AssignedTo = salesUser.Id,
                        CreatedDate = DateTime.UtcNow.AddDays(-30)
                    }
                };

                await context.Opportunities.AddRangeAsync(opps);
                await context.SaveChangesAsync();

                // Seed FollowUps
                var followUps = new List<FollowUp>
                {
                    new FollowUp
                    {
                        CustomerId = customers[0].CustomerId,
                        FollowUpDate = DateTime.Today.AddDays(2).AddHours(10),
                        FollowUpType = "Meeting",
                        Remarks = "Discuss final proposal pricing and SLAs.",
                        Status = "Planned",
                        AssignedTo = salesUser.Id,
                        CreatedDate = DateTime.UtcNow.AddDays(-2)
                    },
                    new FollowUp
                    {
                        LeadId = leads[0].LeadId,
                        FollowUpDate = DateTime.Today.AddDays(1).AddHours(14),
                        FollowUpType = "Call",
                        Remarks = "Initial discovery call on software requirements.",
                        Status = "Planned",
                        AssignedTo = salesUser.Id,
                        CreatedDate = DateTime.UtcNow.AddDays(-1)
                    }
                };

                await context.FollowUps.AddRangeAsync(followUps);

                // Seed Activities
                var activities = new List<Activity>
                {
                    new Activity
                    {
                        ActivityType = "Email",
                        Subject = "Sent updated commercial terms",
                        Description = "Emailed Apex Financial revised cloud infrastructure contract.",
                        ActivityDate = DateTime.Now.AddDays(-1),
                        CustomerId = customers[1].CustomerId,
                        AssignedTo = salesUser.Id,
                        Status = "Completed"
                    },
                    new Activity
                    {
                        ActivityType = "Call",
                        Subject = "Introductory call with Quantum BioTech",
                        Description = "Spoke with lead contact regarding CRM requirements.",
                        ActivityDate = DateTime.Now.AddDays(-3),
                        LeadId = leads[1].LeadId,
                        AssignedTo = salesUser.Id,
                        Status = "Completed"
                    }
                };

                await context.Activities.AddRangeAsync(activities);

                // Seed Initial Audit Logs
                var auditLogs = new List<AuditLog>
                {
                    new AuditLog
                    {
                        UserId = adminUser.Id,
                        UserName = adminUser.UserName!,
                        Action = "LOGIN",
                        EntityName = "Authentication",
                        RecordId = adminUser.Id,
                        CreatedDate = DateTime.UtcNow.AddDays(-1),
                        IpAddress = "127.0.0.1"
                    },
                    new AuditLog
                    {
                        UserId = adminUser.Id,
                        UserName = adminUser.UserName!,
                        Action = "CREATE",
                        EntityName = "Customer",
                        RecordId = "CUST-1001",
                        NewValue = "Acme Global Solutions",
                        CreatedDate = DateTime.UtcNow.AddDays(-1),
                        IpAddress = "127.0.0.1"
                    }
                };

                await context.AuditLogs.AddRangeAsync(auditLogs);
                await context.SaveChangesAsync();
            }
        }
    }
}
