using EnterpriseHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseHub.Infrastructure.Persistence.Oracle;

/// <summary>Reporting schema (ADR-001).</summary>
public sealed class OracleDbContext(DbContextOptions<OracleDbContext> options) : DbContext(options)
{
    public DbSet<ReportSnapshot> ReportSnapshots => Set<ReportSnapshot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ReportSnapshot>(builder =>
        {
            builder.ToTable("REPORT_SNAPSHOTS");
            builder.HasKey(r => r.Id);
            builder.Property(r => r.ReportType).HasMaxLength(100).IsRequired();
            builder.Property(r => r.DataJson).HasColumnType("CLOB").IsRequired();
        });

        modelBuilder.UseClientGeneratedGuidKeys();
        base.OnModelCreating(modelBuilder);
    }
}
