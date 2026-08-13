namespace NexaFlow.Application.DTOs.Workflows;

public sealed record WorkflowResponse(
    Guid Id,
    string Name,
    string? Description,
    string Status,
    Guid CreatedByUserId,
    DateTime CreatedAtUtc,
    int TaskCount);
