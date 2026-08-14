using EnterpriseHub.Application.Projects.Commands;
using FluentValidation;

namespace EnterpriseHub.Application.Projects.Validators;

public sealed class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
    }
}

public sealed class ChangeProjectStatusCommandValidator : AbstractValidator<ChangeProjectStatusCommand>
{
    public ChangeProjectStatusCommandValidator()
    {
        RuleFor(x => x.Status).NotEmpty();
    }
}

public sealed class AddMilestoneCommandValidator : AbstractValidator<AddMilestoneCommand>
{
    public AddMilestoneCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}
