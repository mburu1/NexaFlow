namespace NexaFlow.Application.DTOs.Workflows;

public sealed record UpdateWorkflowRequest(string Name, string? Description, string Status);
