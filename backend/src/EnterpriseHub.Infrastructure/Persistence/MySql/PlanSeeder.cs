using EnterpriseHub.Domain.Billing;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseHub.Infrastructure.Persistence.MySql;

/// <summary>Idempotent seed of the billing plans a tenant can subscribe to — there's no admin UI
/// for plan management yet, so these are fixed at startup rather than left for an empty table.</summary>
public static class PlanSeeder
{
    public static async Task SeedAsync(MySqlDbContext dbContext, CancellationToken ct = default)
    {
        if (await dbContext.Plans.AnyAsync(ct))
            return;

        dbContext.Plans.AddRange(
            Plan.Create("Free", Money.Create(0m), BillingInterval.Monthly),
            Plan.Create("Pro", Money.Create(29m), BillingInterval.Monthly),
            Plan.Create("Business", Money.Create(99m), BillingInterval.Monthly));

        await dbContext.SaveChangesAsync(ct);
    }
}
