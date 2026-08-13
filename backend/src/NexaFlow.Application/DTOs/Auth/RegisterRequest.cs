namespace NexaFlow.Application.DTOs.Auth;

public sealed record RegisterRequest(
    string OrganizationName,
    string Email,
    string Password,
    string FullName);
