namespace NexaFlow.Application.DTOs.Users;

public sealed record UserSummaryResponse(
    Guid Id,
    string Email,
    string FullName,
    string Role,
    bool IsActive,
    DateTime CreatedAtUtc);
