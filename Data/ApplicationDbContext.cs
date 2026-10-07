using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AcxiomCRM.Models;

namespace AcxiomCRM.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Lead> Leads { get; set; }
        public DbSet<Opportunity> Opportunities { get; set; }
        public DbSet<FollowUp> FollowUps { get; set; }
        public DbSet<Activity> Activities { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Customer>()
                .HasIndex(c => c.Email)
                .IsUnique();

            builder.Entity<Customer>()
                .HasIndex(c => c.Phone)
                .IsUnique();

            builder.Entity<Lead>()
                .HasOne(l => l.ConvertedCustomer)
                .WithMany()
                .HasForeignKey(l => l.ConvertedCustomerId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Lead>()
                .HasOne(l => l.ConvertedOpportunity)
                .WithMany()
                .HasForeignKey(l => l.ConvertedOpportunityId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Opportunity>()
                .HasOne(o => o.Customer)
                .WithMany()
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Opportunity>()
                .HasOne(o => o.Lead)
                .WithMany()
                .HasForeignKey(o => o.LeadId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<FollowUp>()
                .HasOne(f => f.Customer)
                .WithMany()
                .HasForeignKey(f => f.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<FollowUp>()
                .HasOne(f => f.Lead)
                .WithMany()
                .HasForeignKey(f => f.LeadId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Activity>()
                .HasOne(a => a.Customer)
                .WithMany()
                .HasForeignKey(a => a.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Activity>()
                .HasOne(a => a.Lead)
                .WithMany()
                .HasForeignKey(a => a.LeadId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
