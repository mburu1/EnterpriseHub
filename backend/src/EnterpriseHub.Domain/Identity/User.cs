using EnterpriseHub.Domain.Common;
using EnterpriseHub.Domain.Identity.Events;

namespace EnterpriseHub.Domain.Identity;

public sealed class User : AggregateRoot<Guid>, ITenantScoped
{
    public Guid TenantId { get; private set; }
    public Email Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public TenantRole Role { get; private set; }
    public bool IsActive { get; private set; } = true;

    private readonly List<RefreshToken> _refreshTokens = [];
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    private User() { }

    public static User Register(Guid tenantId, Email email, string passwordHash, string firstName, string lastName, TenantRole role)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Password hash cannot be empty.");
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("First name is required.");
        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("Last name is required.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Email = email,
            PasswordHash = passwordHash,
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Role = role
        };

        user.Raise(new UserRegisteredEvent(user.Id, tenantId, email.Value));
        return user;
    }

    public RefreshToken IssueRefreshToken(string tokenHash, TimeSpan lifetime)
    {
        var token = RefreshToken.Create(Id, tokenHash, DateTimeOffset.UtcNow.Add(lifetime));
        _refreshTokens.Add(token);
        return token;
    }

    public void Deactivate()
    {
        IsActive = false;
        Touch();
    }
}
