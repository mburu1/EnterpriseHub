using EnterpriseHub.Domain.Billing;

namespace EnterpriseHub.Application.Billing.Dtos;

internal static class BillingMappingExtensions
{
    public static PlanDto ToDto(this Plan plan) =>
        new(plan.Id, plan.Name, plan.Price.Amount, plan.Price.Currency, plan.Interval.ToString());

    public static SubscriptionDto ToDto(this Subscription subscription) =>
        new(subscription.Id, subscription.TenantId, subscription.PlanId, subscription.Status.ToString(), subscription.CurrentPeriodEnd);
}
