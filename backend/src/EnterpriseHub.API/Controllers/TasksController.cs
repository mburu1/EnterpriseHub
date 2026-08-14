using EnterpriseHub.API.Contracts.Projects;
using EnterpriseHub.Application.Common.Messaging;
using EnterpriseHub.Application.Projects.Commands;
using EnterpriseHub.Application.Projects.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseHub.API.Controllers;

[ApiController]
[Route("tasks")]
[Authorize]
public sealed class TasksController(ISender sender) : ControllerBase
{
    [HttpPost("{taskId:guid}/assign")]
    public async Task<ActionResult<ProjectTaskDto>> Assign(Guid taskId, AssignTaskRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new AssignTaskCommand(taskId, request.AssigneeId), ct);
        return Ok(result);
    }

    [HttpPatch("{taskId:guid}/status")]
    public async Task<ActionResult<ProjectTaskDto>> ChangeStatus(Guid taskId, ChangeTaskStatusRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new ChangeTaskStatusCommand(taskId, request.Status), ct);
        return Ok(result);
    }
}
