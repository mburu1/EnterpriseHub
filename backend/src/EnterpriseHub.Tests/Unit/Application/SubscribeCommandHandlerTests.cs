using EnterpriseHub.Application.Billing.Commands;
using EnterpriseHub.Application.Common.Interfaces;
using EnterpriseHub.Domain.Billing;
using EnterpriseHub.Domain.Common;
using NSubstitute;

namespace EnterpriseHub.Tests.Unit.Application;

public class SubscribeCommandHandlerTests
{
    private readonly IPlanRepository _planRepository = Substitute.For<IPlanRepository>();
    private readonly ISubscriptionRepository _subscriptionRepository = Substitute.For<ISubscriptionRepository>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly IBillingUnitOfWork _unitOfWork = Substitute.For<IBillingUnitOfWork>();

    private SubscribeCommandHandler CreateHandler() =>
        new(_planRepository, _subscriptionRepository, _currentUser, _unitOfWork);

    [Fact]
    public async Task Handle_WithNoExistingSubscription_CreatesAndActivatesOne()
    {
        var tenantId = Guid.NewGuid();
        var plan = Plan.Create("Pro", Money.Create(29m), BillingInterval.Monthly);
        _currentUser.TenantId.Returns(tenantId);
        _planRepository.GetByIdAsync(plan.Id, Arg.Any<CancellationToken>()).Returns(plan);
        _subscriptionRepository.GetByTenantIdAsync(tenantId, Arg.Any<CancellationToken>()).Returns((Subscription?)null);

        var result = await CreateHandler().Handle(new SubscribeCommand(plan.Id), CancellationToken.None);

        Assert.Equal(tenantId, result.TenantId);
        Assert.Equal(plan.Id, result.PlanId);
        Assert.Equal(nameof(SubscriptionStatus.Active), result.Status);
        await _subscriptionRepository.Received(1).AddAsync(Arg.Any<Subscription>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithAnActiveSubscriptionAlready_ThrowsDomainException()
    {
        var tenantId = Guid.NewGuid();
        var plan = Plan.Create("Pro", Money.Create(29m), BillingInterval.Monthly);
        var existing = Subscription.Create(tenantId, Guid.NewGuid(), DateTimeOffset.UtcNow.AddMonths(1));
        existing.Activate();

        _currentUser.TenantId.Returns(tenantId);
        _planRepository.GetByIdAsync(plan.Id, Arg.Any<CancellationToken>()).Returns(plan);
        _subscriptionRepository.GetByTenantIdAsync(tenantId, Arg.Any<CancellationToken>()).Returns(existing);

        await Assert.ThrowsAsync<DomainException>(() => CreateHandler().Handle(new SubscribeCommand(plan.Id), CancellationToken.None));
        await _subscriptionRepository.DidNotReceive().AddAsync(Arg.Any<Subscription>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithUnknownPlan_ThrowsDomainException()
    {
        var tenantId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        _currentUser.TenantId.Returns(tenantId);
        _planRepository.GetByIdAsync(planId, Arg.Any<CancellationToken>()).Returns((Plan?)null);

        await Assert.ThrowsAsync<DomainException>(() => CreateHandler().Handle(new SubscribeCommand(planId), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithNoTenantContext_ThrowsForbiddenException()
    {
        _currentUser.TenantId.Returns((Guid?)null);

        await Assert.ThrowsAsync<ForbiddenException>(() => CreateHandler().Handle(new SubscribeCommand(Guid.NewGuid()), CancellationToken.None));
    }
}
