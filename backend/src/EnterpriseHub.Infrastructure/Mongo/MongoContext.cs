using MongoDB.Driver;

namespace EnterpriseHub.Infrastructure.Mongo;

/// <summary>Unstructured activity feeds and document attachments store (ADR-001).</summary>
public sealed class MongoContext
{
    public IMongoDatabase Database { get; }

    public MongoContext(string connectionString, string databaseName)
    {
        var client = new MongoClient(connectionString);
        Database = client.GetDatabase(databaseName);
    }

    public IMongoCollection<NotificationDocument> Notifications =>
        Database.GetCollection<NotificationDocument>("notifications");
}
