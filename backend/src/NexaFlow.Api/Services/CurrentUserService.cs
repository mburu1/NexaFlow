using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using NexaFlow.Application.Common.Interfaces;

namespace NexaFlow.Api.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public Guid? UserId =>
        Guid.TryParse(Principal?.FindFirstValue(JwtRegisteredClaimNames.Sub), out var id) ? id : null;

    public Guid? TenantId =>
        Guid.TryParse(Principal?.FindFirstValue("tenant_id"), out var id) ? id : null;

    public string? Email => Principal?.FindFirstValue(JwtRegisteredClaimNames.Email);

    public string? Role => Principal?.FindFirstValue("role");
}
