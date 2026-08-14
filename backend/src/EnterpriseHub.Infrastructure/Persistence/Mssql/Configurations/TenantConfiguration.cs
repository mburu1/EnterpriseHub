using EnterpriseHub.Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterpriseHub.Infrastructure.Persistence.Mssql.Configurations;

public sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name).HasMaxLength(200).IsRequired();
        builder.Property(t => t.Slug).HasMaxLength(200).IsRequired();
        builder.HasIndex(t => t.Slug).IsUnique();
        builder.Property(t => t.SubscriptionTier).HasConversion<string>().HasMaxLength(20);

        builder.HasMany(t => t.Invitations)
            .WithOne()
            .HasForeignKey(i => i.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(t => t.Invitations).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Ignore(t => t.DomainEvents);
    }
}
