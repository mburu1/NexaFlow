namespace NexaFlow.Application.DTOs.WorkflowTasks;

public sealed record WorkflowTaskResponse(
    Guid Id,
    Guid WorkflowId,
    string Title,
    string? Description,
    string Status,
    Guid? AssignedToUserId,
    DateTime? DueAtUtc,
    DateTime CreatedAtUtc);
