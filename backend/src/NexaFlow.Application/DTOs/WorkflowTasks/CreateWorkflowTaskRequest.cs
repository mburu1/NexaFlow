namespace NexaFlow.Application.DTOs.WorkflowTasks;

public sealed record CreateWorkflowTaskRequest(
    string Title,
    string? Description,
    Guid? AssignedToUserId,
    DateTime? DueAtUtc);
