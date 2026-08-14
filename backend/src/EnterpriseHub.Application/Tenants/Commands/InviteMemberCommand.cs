namespace EnterpriseHub.Application.Tenants.Commands;

using EnterpriseHub.Application.Common.Interfaces;
using EnterpriseHub.Application.Common.Messaging;
using EnterpriseHub.Application.Tenants.Dtos;
using EnterpriseHub.Domain.Common;
using EnterpriseHub.Domain.Identity;
using EnterpriseHub.Domain.Tenants;

public sealed record InviteMemberCommand(string Email, string Role) : ICommand<TenantInvitationDto>;

public sealed class InviteMemberCommandHandler(
    ITenantRepository tenantRepository,
    ICurrentUserService currentUser,
    IUnitOfWork unitOfWork)
    : ICommandHandler<InviteMemberCommand, TenantInvitationDto>
{
    public async Task<TenantInvitationDto> Handle(InviteMemberCommand command, CancellationToken ct)
    {
        if (currentUser.Role is not (nameof(TenantRole.Owner) or nameof(TenantRole.Admin)))
            throw new ForbiddenException("Only tenant owners or admins can invite members.");

        var tenantId = currentUser.TenantId ?? throw new ForbiddenException("No tenant context on the current user.");

        if (!Enum.TryParse<TenantRole>(command.Role, ignoreCase: true, out var role))
            throw new DomainException($"'{command.Role}' is not a valid role.");

        var tenant = await tenantRepository.GetByIdAsync(tenantId, ct)
            ?? throw new DomainException("Tenant not found.");

        var invitation = tenant.InviteMember(command.Email, role);

        await unitOfWork.SaveChangesAsync(ct);

        return new TenantInvitationDto(invitation.Id, invitation.TenantId, invitation.Email,
            invitation.Role.ToString(), invitation.Accepted, invitation.ExpiresAt);
    }
}
