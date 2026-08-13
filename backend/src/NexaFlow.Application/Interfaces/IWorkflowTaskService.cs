using NexaFlow.Application.DTOs.WorkflowTasks;

namespace NexaFlow.Application.Interfaces;

public interface IWorkflowTaskService
{
    Task<IReadOnlyList<WorkflowTaskResponse>> ListByWorkflowAsync(Guid workflowId, CancellationToken cancellationToken = default);

    Task<WorkflowTaskResponse> GetByIdAsync(Guid workflowId, Guid taskId, CancellationToken cancellationToken = default);

    Task<WorkflowTaskResponse> CreateAsync(Guid workflowId, CreateWorkflowTaskRequest request, CancellationToken cancellationToken = default);

    Task<WorkflowTaskResponse> UpdateAsync(Guid workflowId, Guid taskId, UpdateWorkflowTaskRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid workflowId, Guid taskId, CancellationToken cancellationToken = default);
}
