using EnterpriseHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseHub.Infrastructure.Persistence.Postgres;

/// <summary>Analytics and audit log store (ADR-001): landing zone for Kafka-streamed domain events.</summary>
public sealed class PostgresDbContext(DbContextOptions<PostgresDbContext> options) : DbContext(options)
{
    public DbSet<AuditLogEntry> AuditLog => Set<AuditLogEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLogEntry>(builder =>
        {
            builder.ToTable("audit_log");
            builder.HasKey(a => a.Id);
            builder.Property(a => a.EventType).HasMaxLength(200).IsRequired();
            builder.Property(a => a.PayloadJson).IsRequired();
            builder.HasIndex(a => new { a.TenantId, a.OccurredOn });
        });

        modelBuilder.UseClientGeneratedGuidKeys();
        base.OnModelCreating(modelBuilder);
    }
}
