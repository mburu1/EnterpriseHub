using EnterpriseHub.Domain.Projects;

namespace EnterpriseHub.Application.Projects.Dtos;

internal static class ProjectMappingExtensions
{
    public static MilestoneDto ToDto(this Milestone milestone) =>
        new(milestone.Id, milestone.ProjectId, milestone.Name, milestone.DueDate, milestone.IsCompleted);

    public static ProjectDto ToDto(this Project project) =>
        new(project.Id, project.TenantId, project.Name, project.Description, project.Status.ToString(),
            project.Milestones.Select(m => m.ToDto()).ToList());

    public static ProjectTaskDto ToDto(this ProjectTask task) =>
        new(task.Id, task.TenantId, task.ProjectId, task.Title, task.Description, task.Status.ToString(),
            task.Priority.ToString(), task.AssigneeId, task.DueDate);
}
