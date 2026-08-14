using EnterpriseHub.Domain.Common;

namespace EnterpriseHub.Domain.Notifications;

public sealed class Notification : Entity<Guid>, ITenantScoped
{
    public Guid TenantId { get; private set; }
    public Guid UserId { get; private set; }
    public NotificationType Type { get; private set; }
    public string Message { get; private set; } = null!;
    public bool IsRead { get; private set; }

    private Notification() { }

    public static Notification Create(Guid tenantId, Guid userId, NotificationType type, string message) => new()
    {
        Id = Guid.NewGuid(),
        TenantId = tenantId,
        UserId = userId,
        Type = type,
        Message = message
    };

    public void MarkAsRead()
    {
        IsRead = true;
        Touch();
    }

    /// <summary>Reconstructs a Notification from a persistence document; not for creating new notifications.</summary>
    public static Notification Rehydrate(Guid id, Guid tenantId, Guid userId, NotificationType type, string message,
        bool isRead, DateTimeOffset createdAt, DateTimeOffset? updatedAt)
    {
        var notification = new Notification
        {
            Id = id,
            TenantId = tenantId,
            UserId = userId,
            Type = type,
            Message = message,
            IsRead = isRead
        };
        notification.SetTimestamps(createdAt, updatedAt);
        return notification;
    }
}
