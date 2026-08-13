namespace NexaFlow.Application.Common.Interfaces;

/// <summary>
/// Reads the authenticated caller's identity out of JWT claims. TenantId is null when
/// there is no authenticated caller (e.g. during /auth/register or /auth/login), which
/// NexaFlowDbContext's tenant query filter treats as "no restriction" rather than
/// "match nothing" — see ADR-003.
/// </summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }
    Guid? TenantId { get; }
    string? Email { get; }
    string? Role { get; }
    bool IsAuthenticated { get; }
}
