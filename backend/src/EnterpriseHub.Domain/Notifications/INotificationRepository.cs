namespace EnterpriseHub.Domain.Notifications;

public interface INotificationRepository
{
    Task<IReadOnlyList<Notification>> ListByUserAsync(Guid userId, Guid tenantId, CancellationToken ct = default);
    Task AddAsync(Notification notification, CancellationToken ct = default);
    Task MarkAsReadAsync(Guid notificationId, Guid tenantId, CancellationToken ct = default);
}
