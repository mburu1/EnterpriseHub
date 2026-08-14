using EnterpriseHub.Domain.Common;
using EnterpriseHub.Domain.Identity;

namespace EnterpriseHub.Domain.Tenants;

public sealed class TenantInvitation : Entity<Guid>, ITenantScoped
{
    public Guid TenantId { get; private set; }
    public string Email { get; private set; } = null!;
    public TenantRole Role { get; private set; }
    public bool Accepted { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }

    private TenantInvitation() { }

    public static TenantInvitation Create(Guid tenantId, string email, TenantRole role) => new()
    {
        Id = Guid.NewGuid(),
        TenantId = tenantId,
        Email = email.Trim().ToLowerInvariant(),
        Role = role,
        ExpiresAt = DateTimeOffset.UtcNow.AddDays(7)
    };

    public void Accept()
    {
        if (DateTimeOffset.UtcNow > ExpiresAt)
            throw new DomainException("Invitation has expired.");
        Accepted = true;
        Touch();
    }
}
