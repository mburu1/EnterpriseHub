using EnterpriseHub.API.Contracts.Billing;
using EnterpriseHub.Application.Billing.Commands;
using EnterpriseHub.Application.Billing.Dtos;
using EnterpriseHub.Application.Billing.Queries;
using EnterpriseHub.Application.Common.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseHub.API.Controllers;

[ApiController]
[Route("billing")]
[Authorize]
public sealed class BillingController(ISender sender) : ControllerBase
{
    [HttpGet("plans")]
    public async Task<ActionResult<IReadOnlyList<PlanDto>>> ListPlans(CancellationToken ct) =>
        Ok(await sender.Send(new ListPlansQuery(), ct));

    [HttpGet("subscription")]
    public async Task<ActionResult<SubscriptionDto?>> GetSubscription(CancellationToken ct) =>
        Ok(await sender.Send(new GetSubscriptionQuery(), ct));

    [HttpPost("subscription")]
    public async Task<ActionResult<SubscriptionDto>> Subscribe(SubscribeRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new SubscribeCommand(request.PlanId), ct);
        return Ok(result);
    }

    [HttpPost("subscription/cancel")]
    public async Task<ActionResult<SubscriptionDto>> CancelSubscription(CancellationToken ct)
    {
        var result = await sender.Send(new CancelSubscriptionCommand(), ct);
        return Ok(result);
    }
}
