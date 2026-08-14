using EnterpriseHub.Domain.Billing;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseHub.Infrastructure.Persistence.MySql;

/// <summary>Billing and subscription records (ADR-001).</summary>
public sealed class MySqlDbContext(DbContextOptions<MySqlDbContext> options) : DbContext(options)
{
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Plan>(builder =>
        {
            builder.ToTable("Plans");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Name).HasMaxLength(100).IsRequired();
            builder.Property(p => p.Interval).HasConversion<string>().HasMaxLength(20);
            builder.OwnsOne(p => p.Price, money =>
            {
                money.Property(m => m.Amount).HasColumnName("PriceAmount").HasColumnType("decimal(10,2)");
                money.Property(m => m.Currency).HasColumnName("PriceCurrency").HasMaxLength(3);
            });
        });

        modelBuilder.Entity<Subscription>(builder =>
        {
            builder.ToTable("Subscriptions");
            builder.HasKey(s => s.Id);
            builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(20);
            builder.HasIndex(s => s.TenantId).IsUnique();
            builder.Ignore(s => s.DomainEvents);
        });

        base.OnModelCreating(modelBuilder);
    }
}
