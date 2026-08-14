using EnterpriseHub.Application.Common.Interfaces;
using EnterpriseHub.Application.Identity.Commands;
using EnterpriseHub.Domain.Common;
using EnterpriseHub.Domain.Identity;
using EnterpriseHub.Domain.Tenants;
using NSubstitute;

namespace EnterpriseHub.Tests.Unit.Application;

public class RegisterUserCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly ITenantRepository _tenantRepository = Substitute.For<ITenantRepository>();
    private readonly IRefreshTokenRepository _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenGenerator _tokenGenerator = Substitute.For<IJwtTokenGenerator>();
    private readonly IEmailSender _emailSender = Substitute.For<IEmailSender>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private RegisterUserCommandHandler CreateHandler() => new(
        _userRepository, _tenantRepository, _refreshTokenRepository,
        _passwordHasher, _tokenGenerator, _emailSender, _unitOfWork);

    [Fact]
    public async Task Handle_WithNewEmail_CreatesTenantAndOwnerUser()
    {
        _userRepository.ExistsByEmailAsync("owner@acme.com", Arg.Any<CancellationToken>()).Returns(false);
        _tenantRepository.SlugExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _passwordHasher.Hash("Password1").Returns("hashed-password");
        _tokenGenerator.GenerateAccessToken(Arg.Any<User>())
            .Returns(new AccessTokenResult("access-token", DateTimeOffset.UtcNow.AddMinutes(15)));
        _tokenGenerator.GenerateRefreshToken().Returns("raw-refresh-token");
        _tokenGenerator.HashRefreshToken("raw-refresh-token").Returns("hashed-refresh-token");

        var command = new RegisterUserCommand("Acme Inc", "owner@acme.com", "Password1", "Ada", "Lovelace");
        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal("raw-refresh-token", result.RefreshToken);
        Assert.Equal("owner@acme.com", result.User.Email);
        Assert.Equal(nameof(TenantRole.Owner), result.User.Role);

        await _tenantRepository.Received(1).AddAsync(Arg.Any<Tenant>(), Arg.Any<CancellationToken>());
        await _userRepository.Received(1).AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _emailSender.Received(1).SendAsync("owner@acme.com", Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithExistingEmail_ThrowsDomainException()
    {
        _userRepository.ExistsByEmailAsync("owner@acme.com", Arg.Any<CancellationToken>()).Returns(true);

        var command = new RegisterUserCommand("Acme Inc", "owner@acme.com", "Password1", "Ada", "Lovelace");

        await Assert.ThrowsAsync<DomainException>(() => CreateHandler().Handle(command, CancellationToken.None));
        await _tenantRepository.DidNotReceive().AddAsync(Arg.Any<Tenant>(), Arg.Any<CancellationToken>());
    }
}
