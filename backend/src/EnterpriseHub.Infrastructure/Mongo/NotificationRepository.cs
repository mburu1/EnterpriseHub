using EnterpriseHub.Domain.Notifications;
using MongoDB.Driver;

namespace EnterpriseHub.Infrastructure.Mongo;

public sealed class NotificationRepository(MongoContext mongo) : INotificationRepository
{
    public async Task<IReadOnlyList<Notification>> ListByUserAsync(Guid userId, Guid tenantId, CancellationToken ct = default)
    {
        var filter = Builders<NotificationDocument>.Filter.Eq(d => d.UserId, userId) &
                     Builders<NotificationDocument>.Filter.Eq(d => d.TenantId, tenantId);

        var documents = await mongo.Notifications.Find(filter).ToListAsync(ct);
        return documents.Select(ToDomain).ToList();
    }

    public Task AddAsync(Notification notification, CancellationToken ct = default) =>
        mongo.Notifications.InsertOneAsync(ToDocument(notification), cancellationToken: ct);

    public Task MarkAsReadAsync(Guid notificationId, Guid tenantId, CancellationToken ct = default)
    {
        var filter = Builders<NotificationDocument>.Filter.Eq(d => d.Id, notificationId) &
                     Builders<NotificationDocument>.Filter.Eq(d => d.TenantId, tenantId);
        var update = Builders<NotificationDocument>.Update
            .Set(d => d.IsRead, true)
            .Set(d => d.UpdatedAt, DateTimeOffset.UtcNow);

        return mongo.Notifications.UpdateOneAsync(filter, update, cancellationToken: ct);
    }

    private static NotificationDocument ToDocument(Notification notification) => new()
    {
        Id = notification.Id,
        TenantId = notification.TenantId,
        UserId = notification.UserId,
        Type = notification.Type.ToString(),
        Message = notification.Message,
        IsRead = notification.IsRead,
        CreatedAt = notification.CreatedAt,
        UpdatedAt = notification.UpdatedAt
    };

    private static Notification ToDomain(NotificationDocument doc) => Notification.Rehydrate(
        doc.Id, doc.TenantId, doc.UserId, Enum.Parse<NotificationType>(doc.Type), doc.Message,
        doc.IsRead, doc.CreatedAt, doc.UpdatedAt);
}
