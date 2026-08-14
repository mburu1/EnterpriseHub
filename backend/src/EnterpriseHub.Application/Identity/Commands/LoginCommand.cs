namespace EnterpriseHub.Application.Identity.Commands;

using EnterpriseHub.Application.Common.Interfaces;
using EnterpriseHub.Application.Common.Messaging;
using EnterpriseHub.Application.Identity.Dtos;
using EnterpriseHub.Domain.Common;
using EnterpriseHub.Domain.Identity;

public sealed record LoginCommand(string Email, string Password) : ICommand<AuthResponse>;

public sealed class LoginCommandHandler(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator tokenGenerator,
    IUnitOfWork unitOfWork)
    : ICommandHandler<LoginCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(LoginCommand command, CancellationToken ct)
    {
        var user = await userRepository.GetByEmailAsync(command.Email, ct)
            ?? throw new DomainException("Invalid email or password.");

        if (!user.IsActive || !passwordHasher.Verify(command.Password, user.PasswordHash))
            throw new DomainException("Invalid email or password.");

        var accessToken = tokenGenerator.GenerateAccessToken(user);
        var rawRefreshToken = tokenGenerator.GenerateRefreshToken();
        var refreshTokenHash = tokenGenerator.HashRefreshToken(rawRefreshToken);
        var refreshToken = user.IssueRefreshToken(refreshTokenHash, TimeSpan.FromDays(7));
        await refreshTokenRepository.AddAsync(refreshToken, ct);

        await unitOfWork.SaveChangesAsync(ct);

        return new AuthResponse(
            accessToken.Token,
            accessToken.ExpiresAt,
            rawRefreshToken,
            new UserDto(user.Id, user.TenantId, user.Email.Value, user.FirstName, user.LastName, user.Role.ToString()));
    }
}
