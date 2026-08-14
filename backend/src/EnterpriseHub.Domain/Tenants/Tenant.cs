using EnterpriseHub.Domain.Common;
using EnterpriseHub.Domain.Tenants.Events;

namespace EnterpriseHub.Domain.Tenants;

public sealed class Tenant : AggregateRoot<Guid>
{
    public string Name { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public SubscriptionTier SubscriptionTier { get; private set; } = SubscriptionTier.Free;
    public bool IsActive { get; private set; } = true;

    private readonly List<TenantInvitation> _invitations = [];
    public IReadOnlyCollection<TenantInvitation> Invitations => _invitations.AsReadOnly();

    private Tenant() { }

    public static Tenant Create(string name, string slug)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Tenant name is required.");
        if (string.IsNullOrWhiteSpace(slug))
            throw new DomainException("Tenant slug is required.");

        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Slug = slug.Trim().ToLowerInvariant()
        };

        tenant.Raise(new TenantCreatedEvent(tenant.Id, tenant.Name));
        return tenant;
    }

    public TenantInvitation InviteMember(string email, Domain.Identity.TenantRole role)
    {
        var invitation = TenantInvitation.Create(Id, email, role);
        _invitations.Add(invitation);
        return invitation;
    }

    public void UpgradeSubscription(SubscriptionTier tier)
    {
        SubscriptionTier = tier;
        Touch();
    }
}
