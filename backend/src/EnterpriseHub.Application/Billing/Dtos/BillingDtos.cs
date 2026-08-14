namespace EnterpriseHub.Application.Billing.Dtos;

public sealed record PlanDto(Guid Id, string Name, decimal Price, string Currency, string Interval);

public sealed record SubscriptionDto(Guid Id, Guid TenantId, Guid PlanId, string Status, DateTimeOffset CurrentPeriodEnd);
