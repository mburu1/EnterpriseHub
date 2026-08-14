namespace EnterpriseHub.Application.Identity.Commands;

using EnterpriseHub.Application.Common.Interfaces;
using EnterpriseHub.Application.Common.Messaging;
using EnterpriseHub.Application.Identity.Dtos;
using EnterpriseHub.Domain.Common;
using EnterpriseHub.Domain.Identity;
using EnterpriseHub.Domain.Tenants;
using Microsoft.Extensions.Logging;

public sealed record RegisterUserCommand(
    string OrganizationName,
    string Email,
    string Password,
    string FirstName,
    string LastName) : ICommand<AuthResponse>;

public sealed class RegisterUserCommandHandler(
    IUserRepository userRepository,
    ITenantRepository tenantRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator tokenGenerator,
    IEmailSender emailSender,
    IUnitOfWork unitOfWork,
    ILogger<RegisterUserCommandHandler> logger)
    : ICommandHandler<RegisterUserCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(RegisterUserCommand command, CancellationToken ct)
    {
        if (await userRepository.ExistsByEmailAsync(command.Email, ct))
            throw new DomainException($"An account with email '{command.Email}' already exists.");

        var slug = await GenerateUniqueSlugAsync(command.OrganizationName, ct);
        var tenant = Tenant.Create(command.OrganizationName, slug);
        await tenantRepository.AddAsync(tenant, ct);

        var email = Email.Create(command.Email);
        var passwordHash = passwordHasher.Hash(command.Password);
        var user = User.Register(tenant.Id, email, passwordHash, command.FirstName, command.LastName, TenantRole.Owner);
        await userRepository.AddAsync(user, ct);

        var accessToken = tokenGenerator.GenerateAccessToken(user);
        var rawRefreshToken = tokenGenerator.GenerateRefreshToken();
        var refreshTokenHash = tokenGenerator.HashRefreshToken(rawRefreshToken);
        var refreshToken = user.IssueRefreshToken(refreshTokenHash, TimeSpan.FromDays(7));
        await refreshTokenRepository.AddAsync(refreshToken, ct);

        await unitOfWork.SaveChangesAsync(ct);

        // Best-effort: the account is already committed at this point, so an SMTP outage must not
        // turn a successful signup into a reported failure for the caller.
        try
        {
            await emailSender.SendAsync(
                email.Value,
                "Welcome to EnterpriseHub",
                $"<p>Hi {user.FirstName}, your organization <strong>{tenant.Name}</strong> is ready.</p>",
                ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send welcome email to {Email}.", email.Value);
        }

        return new AuthResponse(
            accessToken.Token,
            accessToken.ExpiresAt,
            rawRefreshToken,
            new UserDto(user.Id, user.TenantId, email.Value, user.FirstName, user.LastName, user.Role.ToString()));
    }

    private async Task<string> GenerateUniqueSlugAsync(string name, CancellationToken ct)
    {
        var baseSlug = new string([.. name.Trim().ToLowerInvariant().Select(c => char.IsLetterOrDigit(c) ? c : '-')]);
        var slug = baseSlug;
        var suffix = 1;
        while (await tenantRepository.SlugExistsAsync(slug, ct))
            slug = $"{baseSlug}-{suffix++}";
        return slug;
    }
}
