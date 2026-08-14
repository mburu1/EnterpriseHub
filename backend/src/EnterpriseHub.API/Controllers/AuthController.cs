using System.Security.Claims;
using EnterpriseHub.API.Contracts.Auth;
using EnterpriseHub.Application.Common.Messaging;
using EnterpriseHub.Application.Identity.Commands;
using EnterpriseHub.Application.Identity.Dtos;
using EnterpriseHub.Application.Identity.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseHub.API.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthController(ISender sender) : ControllerBase
{
    /// <summary>Creates a new organization and its owner user, returning an access + refresh token pair.</summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken ct)
    {
        var command = new RegisterUserCommand(request.OrganizationName, request.Email, request.Password, request.FirstName, request.LastName);
        var result = await sender.Send(command, ct);
        return Ok(result);
    }

    /// <summary>Exchanges credentials for an access + refresh token pair.</summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new LoginCommand(request.Email, request.Password), ct);
        return Ok(result);
    }

    /// <summary>Rotates a refresh token for a new access + refresh token pair.</summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new RefreshTokenCommand(request.RefreshToken), ct);
        return Ok(result);
    }

    /// <summary>Returns the authenticated user; verifies the access token round-trips through [Authorize].</summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> Me(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
        var result = await sender.Send(new GetCurrentUserQuery(userId), ct);
        return Ok(result);
    }
}
