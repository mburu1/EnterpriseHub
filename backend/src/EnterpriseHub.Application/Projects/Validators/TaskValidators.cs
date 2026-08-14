using EnterpriseHub.Application.Projects.Commands;
using FluentValidation;

namespace EnterpriseHub.Application.Projects.Validators;

public sealed class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Priority).NotEmpty();
    }
}

public sealed class ChangeTaskStatusCommandValidator : AbstractValidator<ChangeTaskStatusCommand>
{
    public ChangeTaskStatusCommandValidator()
    {
        RuleFor(x => x.Status).NotEmpty();
    }
}
