namespace EnterpriseHub.Domain.Common;

public interface ITenantScoped
{
    Guid TenantId { get; }
}
