namespace NexaFlow.Application.DTOs.Auth;

public sealed record UserResponse(
    Guid Id,
    string Email,
    string FullName,
    string Role,
    Guid TenantId,
    string TenantName);
