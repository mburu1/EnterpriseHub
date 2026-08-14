using EnterpriseHub.Application.Common.Interfaces;

namespace EnterpriseHub.Infrastructure.Identity;

public sealed class PasswordHasher : IPasswordHasher
{
    public string Hash(string password) => BCrypt.Net.BCrypt.EnhancedHashPassword(password, workFactor: 12);

    public bool Verify(string password, string hash) => BCrypt.Net.BCrypt.EnhancedVerify(password, hash);
}
