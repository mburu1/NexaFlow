namespace NexaFlow.Application.DTOs.WorkflowTasks;

public sealed record UpdateWorkflowTaskRequest(
    string Title,
    string? Description,
    string Status,
    Guid? AssignedToUserId,
    DateTime? DueAtUtc);
