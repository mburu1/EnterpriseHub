using EnterpriseHub.Domain.Identity;
using EnterpriseHub.Domain.Projects;
using EnterpriseHub.Domain.Tenants;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseHub.Infrastructure.Persistence.Mssql;

/// <summary>Primary relational store: tenants, users, projects, and tasks (ADR-001).</summary>
public sealed class MssqlDbContext(DbContextOptions<MssqlDbContext> options) : DbContext(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<TenantInvitation> TenantInvitations => Set<TenantInvitation>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Milestone> Milestones => Set<Milestone>();
    public DbSet<ProjectTask> ProjectTasks => Set<ProjectTask>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MssqlDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
