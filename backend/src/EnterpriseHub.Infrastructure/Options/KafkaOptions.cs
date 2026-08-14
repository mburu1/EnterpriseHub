namespace EnterpriseHub.Infrastructure.Options;

public sealed class KafkaOptions
{
    public const string SectionName = "Kafka";

    public string BootstrapServers { get; set; } = "localhost:9092";
    public string AuditTopic { get; set; } = "enterprisehub.audit-events";
}
