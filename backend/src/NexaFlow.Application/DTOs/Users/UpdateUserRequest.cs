namespace NexaFlow.Application.DTOs.Users;

public sealed record UpdateUserRequest(string FullName, string Role, bool IsActive);
