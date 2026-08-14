using EnterpriseHub.Domain.Common;

namespace EnterpriseHub.Domain.Identity;

public sealed class RefreshToken : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = null!;
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public Guid? ReplacedByTokenId { get; private set; }

    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt is not null;
    public bool IsActive => !IsExpired && !IsRevoked;

    private RefreshToken() { }

    public static RefreshToken Create(Guid userId, string tokenHash, DateTimeOffset expiresAt) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        TokenHash = tokenHash,
        ExpiresAt = expiresAt
    };

    public void Revoke(Guid? replacedByTokenId = null)
    {
        if (IsRevoked) return;
        RevokedAt = DateTimeOffset.UtcNow;
        ReplacedByTokenId = replacedByTokenId;
    }
}
