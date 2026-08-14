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
}
