namespace EnterpriseHub.Application.Tenants.Queries;

using EnterpriseHub.Application.Common.Interfaces;
using EnterpriseHub.Application.Common.Messaging;
using EnterpriseHub.Application.Tenants.Dtos;
using EnterpriseHub.Domain.Common;
using EnterpriseHub.Domain.Tenants;

public sealed record GetMyTenantQuery : IQuery<TenantDto>;

public sealed class GetMyTenantQueryHandler(ITenantRepository tenantRepository, ICurrentUserService currentUser)
    : IQueryHandler<GetMyTenantQuery, TenantDto>
{
    public async Task<TenantDto> Handle(GetMyTenantQuery query, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId ?? throw new ForbiddenException("No tenant context on the current user.");
        var tenant = await tenantRepository.GetByIdAsync(tenantId, ct)
            ?? throw new DomainException("Tenant not found.");

        return new TenantDto(tenant.Id, tenant.Name, tenant.Slug, tenant.SubscriptionTier.ToString());
    }
}
