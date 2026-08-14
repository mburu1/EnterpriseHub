namespace EnterpriseHub.Application.Billing.Queries;

using EnterpriseHub.Application.Billing.Dtos;
using EnterpriseHub.Application.Common.Messaging;
using EnterpriseHub.Domain.Billing;

public sealed record ListPlansQuery : IQuery<IReadOnlyList<PlanDto>>;

public sealed class ListPlansQueryHandler(IPlanRepository planRepository) : IQueryHandler<ListPlansQuery, IReadOnlyList<PlanDto>>
{
    public async Task<IReadOnlyList<PlanDto>> Handle(ListPlansQuery query, CancellationToken ct)
    {
        var plans = await planRepository.ListAsync(ct);
        return plans.Select(p => p.ToDto()).ToList();
    }
}
