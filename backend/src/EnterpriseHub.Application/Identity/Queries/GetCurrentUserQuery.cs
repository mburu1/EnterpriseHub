namespace EnterpriseHub.Application.Identity.Queries;

using EnterpriseHub.Application.Common.Messaging;
using EnterpriseHub.Application.Identity.Dtos;
using EnterpriseHub.Domain.Common;
using EnterpriseHub.Domain.Identity;

public sealed record GetCurrentUserQuery(Guid UserId) : IQuery<UserDto>;

public sealed class GetCurrentUserQueryHandler(IUserRepository userRepository)
    : IQueryHandler<GetCurrentUserQuery, UserDto>
{
    public async Task<UserDto> Handle(GetCurrentUserQuery query, CancellationToken ct)
    {
        var user = await userRepository.GetByIdAsync(query.UserId, ct)
            ?? throw new DomainException("User not found.");

        return new UserDto(user.Id, user.TenantId, user.Email.Value, user.FirstName, user.LastName, user.Role.ToString());
    }
}
