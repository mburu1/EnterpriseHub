namespace EnterpriseHub.Application.Identity.Commands;

using EnterpriseHub.Application.Common.Interfaces;
using EnterpriseHub.Application.Common.Messaging;
using EnterpriseHub.Application.Identity.Dtos;
using EnterpriseHub.Domain.Common;
using EnterpriseHub.Domain.Identity;

public sealed record RefreshTokenCommand(string RefreshToken) : ICommand<AuthResponse>;

public sealed class RefreshTokenCommandHandler(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IJwtTokenGenerator tokenGenerator,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RefreshTokenCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(RefreshTokenCommand command, CancellationToken ct)
    {
        var hash = tokenGenerator.HashRefreshToken(command.RefreshToken);
        var existingToken = await refreshTokenRepository.GetByHashAsync(hash, ct)
            ?? throw new DomainException("Invalid refresh token.");

        if (!existingToken.IsActive)
            throw new DomainException("Refresh token is expired or has been revoked.");

        var user = await userRepository.GetByIdAsync(existingToken.UserId, ct)
            ?? throw new DomainException("User no longer exists.");

        var newRawRefreshToken = tokenGenerator.GenerateRefreshToken();
        var newRefreshToken = user.IssueRefreshToken(tokenGenerator.HashRefreshToken(newRawRefreshToken), TimeSpan.FromDays(7));
        existingToken.Revoke(newRefreshToken.Id);

        await refreshTokenRepository.AddAsync(newRefreshToken, ct);
        refreshTokenRepository.Update(existingToken);

        var accessToken = tokenGenerator.GenerateAccessToken(user);

        await unitOfWork.SaveChangesAsync(ct);

        return new AuthResponse(
            accessToken.Token,
            accessToken.ExpiresAt,
            newRawRefreshToken,
            new UserDto(user.Id, user.TenantId, user.Email.Value, user.FirstName, user.LastName, user.Role.ToString()));
    }
}
