using FinalProject.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FinalProject.Models; // Add this to include the Email entity

namespace FinalProject.Data
{
    public class FinalProjectContext : IdentityDbContext<FinalProjectUser>
    {
        public FinalProjectContext(DbContextOptions<FinalProjectContext> options)
            : base(options)
        {
        }

        // DbSet for Email to represent the emails table in the database
        public DbSet<Email> Emails { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure FinalProjectUser entity (existing customization)
            builder.ApplyConfiguration(new FinalProjectUserEntityConfiguration());

            // Configure Email entity if needed (optional, for more customization)
            builder.Entity<Email>(entity =>
            {
                entity.HasKey(e => e.Id);  // Primary key configuration
                entity.Property(e => e.EmailReceiver).HasMaxLength(255).IsRequired();
                entity.Property(e => e.Subject).HasMaxLength(255).IsRequired();
                entity.Property(e => e.Body).HasMaxLength(2000);  // Max body length, adjust as needed
                entity.Property(e => e.DateSent).IsRequired();  // DateSent cannot be null
            });
        }
    }

    // Existing configuration for FinalProjectUser
    public class FinalProjectUserEntityConfiguration : IEntityTypeConfiguration<FinalProjectUser>
    {
        public void Configure(EntityTypeBuilder<FinalProjectUser> builder)
        {
            builder.Property(u => u.FirstName).HasMaxLength(255);
            builder.Property(u => u.LastName).HasMaxLength(255);
            builder.Property(u => u.MobilePhone).HasMaxLength(255);
        }
    }
}