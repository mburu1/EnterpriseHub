using EnterpriseHub.API.Contracts.Projects;
using EnterpriseHub.Application.Common.Messaging;
using EnterpriseHub.Application.Projects.Commands;
using EnterpriseHub.Application.Projects.Dtos;
using EnterpriseHub.Application.Projects.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseHub.API.Controllers;

[ApiController]
[Route("projects")]
[Authorize]
public sealed class ProjectsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProjectDto>>> ListProjects(CancellationToken ct) =>
        Ok(await sender.Send(new ListProjectsQuery(), ct));

    [HttpPost]
    public async Task<ActionResult<ProjectDto>> CreateProject(CreateProjectRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new CreateProjectCommand(request.Name, request.Description), ct);
        return Ok(result);
    }

    [HttpGet("{projectId:guid}")]
    public async Task<ActionResult<ProjectDto>> GetProject(Guid projectId, CancellationToken ct) =>
        Ok(await sender.Send(new GetProjectQuery(projectId), ct));

    [HttpPatch("{projectId:guid}/status")]
    public async Task<ActionResult<ProjectDto>> ChangeStatus(Guid projectId, ChangeProjectStatusRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new ChangeProjectStatusCommand(projectId, request.Status), ct);
        return Ok(result);
    }

    [HttpPost("{projectId:guid}/milestones")]
    public async Task<ActionResult<MilestoneDto>> AddMilestone(Guid projectId, AddMilestoneRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new AddMilestoneCommand(projectId, request.Name, request.DueDate), ct);
        return Ok(result);
    }

    [HttpGet("{projectId:guid}/tasks")]
    public async Task<ActionResult<IReadOnlyList<ProjectTaskDto>>> ListTasks(Guid projectId, CancellationToken ct) =>
        Ok(await sender.Send(new ListTasksQuery(projectId), ct));

    [HttpPost("{projectId:guid}/tasks")]
    public async Task<ActionResult<ProjectTaskDto>> CreateTask(Guid projectId, CreateTaskRequest request, CancellationToken ct)
    {
        var command = new CreateTaskCommand(projectId, request.Title, request.Description, request.Priority, request.DueDate);
        var result = await sender.Send(command, ct);
        return Ok(result);
    }
}
