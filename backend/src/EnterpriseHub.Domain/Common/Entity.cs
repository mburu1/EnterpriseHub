namespace EnterpriseHub.Domain.Common;

public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    public TId Id { get; protected set; } = default!;
    public DateTimeOffset CreatedAt { get; protected set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; protected set; }

    protected Entity() { }

    protected Entity(TId id)
    {
        Id = id;
    }

    protected void Touch() => UpdatedAt = DateTimeOffset.UtcNow;

    /// <summary>Restores audit timestamps when rehydrating an entity from a persistence store.</summary>
    protected void SetTimestamps(DateTimeOffset createdAt, DateTimeOffset? updatedAt)
    {
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public bool Equals(Entity<TId>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        return Id.Equals(other.Id);
    }

    public override bool Equals(object? obj) => Equals(obj as Entity<TId>);

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right) =>
        left is null ? right is null : left.Equals(right);

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) => !(left == right);
}
