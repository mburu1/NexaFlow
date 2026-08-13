using NexaFlow.Application.Common.Interfaces;

namespace NexaFlow.Tests.TestUtilities;

public sealed class TestCurrentUserService : ICurrentUserService
{
    public Guid? UserId { get; set; }
    public Guid? TenantId { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; }
    public bool IsAuthenticated => UserId.HasValue;
}
