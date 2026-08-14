using EnterpriseHub.Domain.Projects;
using EnterpriseHub.Domain.Projects.Events;

namespace EnterpriseHub.Tests.Unit.Domain;

public class ProjectTaskTests
{
    private static ProjectTask CreateTask() =>
        ProjectTask.Create(Guid.NewGuid(), Guid.NewGuid(), "Write ADR", null, TaskPriority.High, null);

    [Fact]
    public void AssignTo_RaisesTaskAssignedEvent()
    {
        var task = CreateTask();
        var assigneeId = Guid.NewGuid();

        task.AssignTo(assigneeId);

        var evt = Assert.IsType<TaskAssignedEvent>(Assert.Single(task.DomainEvents));
        Assert.Equal(assigneeId, evt.AssigneeId);
        Assert.Equal(assigneeId, task.AssigneeId);
    }

    [Fact]
    public void ChangeStatus_ToSameStatus_DoesNotRaiseEvent()
    {
        var task = CreateTask();

        task.ChangeStatus(ProjectTaskStatus.Todo);

        Assert.Empty(task.DomainEvents);
    }

    [Fact]
    public void ChangeStatus_ToDifferentStatus_RaisesTaskStatusChangedEvent()
    {
        var task = CreateTask();

        task.ChangeStatus(ProjectTaskStatus.InProgress);

        var evt = Assert.IsType<TaskStatusChangedEvent>(Assert.Single(task.DomainEvents));
        Assert.Equal(ProjectTaskStatus.Todo, evt.PreviousStatus);
        Assert.Equal(ProjectTaskStatus.InProgress, evt.NewStatus);
    }

    [Fact]
    public void Create_WithBlankTitle_ThrowsDomainException()
    {
        Assert.Throws<EnterpriseHub.Domain.Common.DomainException>(() =>
            ProjectTask.Create(Guid.NewGuid(), Guid.NewGuid(), "   ", null, TaskPriority.Low, null));
    }
}
