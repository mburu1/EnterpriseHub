namespace EnterpriseHub.Application.Tenants.Commands;

using EnterpriseHub.Application.Common.Interfaces;
using EnterpriseHub.Application.Common.Messaging;
using EnterpriseHub.Application.Identity.Dtos;
using EnterpriseHub.Domain.Common;
using EnterpriseHub.Domain.Identity;
using EnterpriseHub.Domain.Tenants;

public sealed record AcceptInvitationCommand(
    Guid InvitationId,
    string Password,
    string FirstName,
    string LastName) : ICommand<AuthResponse>;

public sealed class AcceptInvitationCommandHandler(
    ITenantRepository tenantRepository,
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator tokenGenerator,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AcceptInvitationCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(AcceptInvitationCommand command, CancellationToken ct)
    {
        var invitation = await tenantRepository.GetInvitationByIdAsync(command.InvitationId, ct)
            ?? throw new DomainException("Invitation not found.");

        if (invitation.Accepted)
            throw new DomainException("This invitation has already been accepted.");

        if (await userRepository.ExistsByEmailAsync(invitation.Email, ct))
            throw new DomainException($"An account with email '{invitation.Email}' already exists.");

        var email = Email.Create(invitation.Email);
        var passwordHash = passwordHasher.Hash(command.Password);
        var user = User.Register(invitation.TenantId, email, passwordHash, command.FirstName, command.LastName, invitation.Role);
        await userRepository.AddAsync(user, ct);

        invitation.Accept();

        var accessToken = tokenGenerator.GenerateAccessToken(user);
        var rawRefreshToken = tokenGenerator.GenerateRefreshToken();
        var refreshToken = user.IssueRefreshToken(tokenGenerator.HashRefreshToken(rawRefreshToken), TimeSpan.FromDays(7));
        await refreshTokenRepository.AddAsync(refreshToken, ct);

        await unitOfWork.SaveChangesAsync(ct);

        return new AuthResponse(
            accessToken.Token,
            accessToken.ExpiresAt,
            rawRefreshToken,
            new UserDto(user.Id, user.TenantId, email.Value, user.FirstName, user.LastName, user.Role.ToString()));
    }
}
