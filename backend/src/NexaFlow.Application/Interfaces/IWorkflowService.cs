using NexaFlow.Application.DTOs.Workflows;

namespace NexaFlow.Application.Interfaces;

public interface IWorkflowService
{
    Task<IReadOnlyList<WorkflowResponse>> ListAsync(CancellationToken cancellationToken = default);

    Task<WorkflowResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<WorkflowResponse> CreateAsync(CreateWorkflowRequest request, CancellationToken cancellationToken = default);

    Task<WorkflowResponse> UpdateAsync(Guid id, UpdateWorkflowRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
