using EnterpriseHub.Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterpriseHub.Infrastructure.Persistence.Mssql.Configurations;

public sealed class TenantInvitationConfiguration : IEntityTypeConfiguration<TenantInvitation>
{
    public void Configure(EntityTypeBuilder<TenantInvitation> builder)
    {
        builder.ToTable("TenantInvitations");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Email).HasMaxLength(256).IsRequired();
        builder.Property(i => i.Role).HasConversion<string>().HasMaxLength(20);
    }
}
