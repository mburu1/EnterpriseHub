using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EnterpriseHub.Infrastructure.Persistence;

internal static class ModelBuilderExtensions
{
    /// <summary>Every aggregate/entity in this domain self-assigns its Guid Id in its factory method
    /// (Guid.NewGuid()) rather than relying on a database default. Without this, EF Core's
    /// change-tracking heuristic for entities reached via graph-fixup on an already-tracked parent
    /// (e.g. appending a new TenantInvitation to a loaded Tenant) can mistake the new child for an
    /// existing row — since its key is already non-default — and issue an UPDATE instead of an
    /// INSERT, which then fails with a concurrency exception because no such row exists yet.
    /// Entities added directly via DbSet.Add(...) are unaffected (that call is always unambiguous).</summary>
    public static void UseClientGeneratedGuidKeys(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var idProperty = entityType.FindProperty("Id");
            if (idProperty is { ClrType.Name: nameof(Guid) })
                idProperty.ValueGenerated = ValueGenerated.Never;
        }
    }
}
