namespace EnterpriseHub.Application.Tenants.Dtos;

public sealed record TenantDto(Guid Id, string Name, string Slug, string SubscriptionTier);

public sealed record TenantInvitationDto(
    Guid Id,
    Guid TenantId,
    string Email,
    string Role,
    bool Accepted,
    DateTimeOffset ExpiresAt);
