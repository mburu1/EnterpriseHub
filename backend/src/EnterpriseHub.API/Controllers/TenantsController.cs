using EnterpriseHub.API.Contracts.Tenants;
using EnterpriseHub.Application.Common.Messaging;
using EnterpriseHub.Application.Identity.Dtos;
using EnterpriseHub.Application.Tenants.Commands;
using EnterpriseHub.Application.Tenants.Dtos;
using EnterpriseHub.Application.Tenants.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseHub.API.Controllers;

[ApiController]
[Route("tenants")]
[Authorize]
public sealed class TenantsController(ISender sender) : ControllerBase
{
    /// <summary>The authenticated user's own organization.</summary>
    [HttpGet("me")]
    public async Task<ActionResult<TenantDto>> GetMyTenant(CancellationToken ct) =>
        Ok(await sender.Send(new GetMyTenantQuery(), ct));

    /// <summary>Invites a member by email. Requires Owner or Admin role.</summary>
    [HttpPost("invitations")]
    public async Task<ActionResult<TenantInvitationDto>> InviteMember(InviteMemberRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new InviteMemberCommand(request.Email, request.Role), ct);
        return Ok(result);
    }

    /// <summary>Pending and accepted invitations for the authenticated user's tenant.</summary>
    [HttpGet("invitations")]
    public async Task<ActionResult<IReadOnlyList<TenantInvitationDto>>> ListInvitations(CancellationToken ct) =>
        Ok(await sender.Send(new ListInvitationsQuery(), ct));

    /// <summary>Accepts an invitation and creates the invited user's account — public, since the
    /// invitee doesn't have an account (or token) yet.</summary>
    [AllowAnonymous]
    [HttpPost("invitations/{invitationId:guid}/accept")]
    public async Task<ActionResult<AuthResponse>> AcceptInvitation(Guid invitationId, AcceptInvitationRequest request, CancellationToken ct)
    {
        var command = new AcceptInvitationCommand(invitationId, request.Password, request.FirstName, request.LastName);
        var result = await sender.Send(command, ct);
        return Ok(result);
    }
}
