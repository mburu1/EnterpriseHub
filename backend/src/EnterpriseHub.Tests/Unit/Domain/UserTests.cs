using EnterpriseHub.Domain.Identity;
using EnterpriseHub.Domain.Identity.Events;

namespace EnterpriseHub.Tests.Unit.Domain;

public class UserTests
{
    [Fact]
    public void Register_RaisesUserRegisteredEvent()
    {
        var tenantId = Guid.NewGuid();
        var email = Email.Create("owner@acme.com");

        var user = User.Register(tenantId, email, "hashed-password", "Ada", "Lovelace", TenantRole.Owner);

        var raised = Assert.Single(user.DomainEvents);
        var evt = Assert.IsType<UserRegisteredEvent>(raised);
        Assert.Equal(user.Id, evt.UserId);
        Assert.Equal(tenantId, evt.TenantId);
        Assert.Equal("owner@acme.com", evt.Email);
    }

    [Fact]
    public void IssueRefreshToken_AddsActiveTokenToCollection()
    {
        var user = User.Register(Guid.NewGuid(), Email.Create("owner@acme.com"), "hash", "Ada", "Lovelace", TenantRole.Owner);

        var token = user.IssueRefreshToken("token-hash", TimeSpan.FromDays(7));

        Assert.Contains(token, user.RefreshTokens);
        Assert.True(token.IsActive);
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var user = User.Register(Guid.NewGuid(), Email.Create("owner@acme.com"), "hash", "Ada", "Lovelace", TenantRole.Owner);

        user.Deactivate();

        Assert.False(user.IsActive);
    }
}
