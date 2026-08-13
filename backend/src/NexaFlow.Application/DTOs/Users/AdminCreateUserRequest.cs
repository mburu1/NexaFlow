namespace NexaFlow.Application.DTOs.Users;

/// <summary>An Admin/Manager provisioning a teammate directly — distinct from self-serve /auth/register.</summary>
public sealed record AdminCreateUserRequest(string Email, string Password, string FullName, string Role);
