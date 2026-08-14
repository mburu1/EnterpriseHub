namespace EnterpriseHub.Application.Tenants.Queries;

using EnterpriseHub.Application.Common.Interfaces;
using EnterpriseHub.Application.Common.Messaging;
using EnterpriseHub.Application.Tenants.Dtos;
using EnterpriseHub.Domain.Common;
using EnterpriseHub.Domain.Tenants;

public sealed record ListInvitationsQuery : IQuery<IReadOnlyList<TenantInvitationDto>>;

public sealed class ListInvitationsQueryHandler(ITenantRepository tenantRepository, ICurrentUserService currentUser)
    : IQueryHandler<ListInvitationsQuery, IReadOnlyList<TenantInvitationDto>>
{
    public async Task<IReadOnlyList<TenantInvitationDto>> Handle(ListInvitationsQuery query, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId ?? throw new ForbiddenException("No tenant context on the current user.");
        var invitations = await tenantRepository.ListInvitationsAsync(tenantId, ct);

        return invitations
            .Select(i => new TenantInvitationDto(i.Id, i.TenantId, i.Email, i.Role.ToString(), i.Accepted, i.ExpiresAt))
            .ToList();
    }
}
