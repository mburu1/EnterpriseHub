using EnterpriseHub.Application.Common.Interfaces;
using EnterpriseHub.Application.Identity.Commands;
using EnterpriseHub.Domain.Common;
using EnterpriseHub.Domain.Identity;
using NSubstitute;

namespace EnterpriseHub.Tests.Unit.Application;

public class LoginCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IRefreshTokenRepository _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenGenerator _tokenGenerator = Substitute.For<IJwtTokenGenerator>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private LoginCommandHandler CreateHandler() =>
        new(_userRepository, _refreshTokenRepository, _passwordHasher, _tokenGenerator, _unitOfWork);

    private static User CreateActiveUser() =>
        User.Register(Guid.NewGuid(), Email.Create("owner@acme.com"), "hashed-password", "Ada", "Lovelace", TenantRole.Owner);

    [Fact]
    public async Task Handle_WithUnknownEmail_ThrowsDomainException()
    {
        _userRepository.GetByEmailAsync("nobody@acme.com", Arg.Any<CancellationToken>()).Returns((User?)null);

        var command = new LoginCommand("nobody@acme.com", "whatever");

        await Assert.ThrowsAsync<DomainException>(() => CreateHandler().Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithWrongPassword_ThrowsDomainException()
    {
        var user = CreateActiveUser();
        _userRepository.GetByEmailAsync("owner@acme.com", Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify("wrong-password", "hashed-password").Returns(false);

        var command = new LoginCommand("owner@acme.com", "wrong-password");

        await Assert.ThrowsAsync<DomainException>(() => CreateHandler().Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ReturnsAuthResponseAndIssuesRefreshToken()
    {
        var user = CreateActiveUser();
        _userRepository.GetByEmailAsync("owner@acme.com", Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify("Password1", "hashed-password").Returns(true);
        _tokenGenerator.GenerateAccessToken(user)
            .Returns(new AccessTokenResult("access-token", DateTimeOffset.UtcNow.AddMinutes(15)));
        _tokenGenerator.GenerateRefreshToken().Returns("raw-refresh-token");
        _tokenGenerator.HashRefreshToken("raw-refresh-token").Returns("hashed-refresh-token");

        var command = new LoginCommand("owner@acme.com", "Password1");
        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal("raw-refresh-token", result.RefreshToken);
        await _refreshTokenRepository.Received(1).AddAsync(Arg.Any<RefreshToken>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
