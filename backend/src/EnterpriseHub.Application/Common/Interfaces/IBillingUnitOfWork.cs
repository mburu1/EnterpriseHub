namespace EnterpriseHub.Application.Common.Interfaces;

/// <summary>Commits the billing store's (MySQL) change tracker. Separate from <see cref="IUnitOfWork"/>,
/// which wraps the primary store (MSSQL) — each relational DbContext needs its own save boundary
/// since there's no cross-database transaction spanning them (see ADR-001, ADR-002).</summary>
public interface IBillingUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
