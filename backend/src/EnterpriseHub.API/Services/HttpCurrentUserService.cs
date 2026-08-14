using System.Security.Claims;
using EnterpriseHub.Application.Common.Interfaces;

namespace EnterpriseHub.API.Services;

public sealed class HttpCurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public Guid? UserId
    {
        get
        {
            var value = Principal?.FindFirstValue(ClaimTypes.NameIdentifier) ?? Principal?.FindFirstValue("sub");
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public Guid? TenantId
    {
        get
        {
            var value = Principal?.FindFirstValue("tenant_id");
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public string? Role => Principal?.FindFirstValue(ClaimTypes.Role);
}
