using EnterpriseHub.Application.Tenants.Commands;
using FluentValidation;

namespace EnterpriseHub.Application.Tenants.Validators;

public sealed class InviteMemberCommandValidator : AbstractValidator<InviteMemberCommand>
{
    public InviteMemberCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Role).NotEmpty();
    }
}
