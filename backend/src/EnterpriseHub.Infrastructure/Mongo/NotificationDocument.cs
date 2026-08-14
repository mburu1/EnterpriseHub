using MongoDB.Bson.Serialization.Attributes;

namespace EnterpriseHub.Infrastructure.Mongo;

public sealed class NotificationDocument
{
    [BsonId]
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public string Type { get; set; } = null!;
    public string Message { get; set; } = null!;
    public bool IsRead { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
