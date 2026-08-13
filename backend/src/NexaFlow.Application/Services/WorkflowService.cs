using NexaFlow.Application.Common.Exceptions;
using NexaFlow.Application.Common.Interfaces;
using NexaFlow.Application.DTOs.Workflows;
using NexaFlow.Application.Interfaces;
using NexaFlow.Domain.Entities;
using NexaFlow.Domain.Enums;
using NexaFlow.Domain.Interfaces;

namespace NexaFlow.Application.Services;

public sealed class WorkflowService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService) : IWorkflowService
{
    public async Task<IReadOnlyList<WorkflowResponse>> ListAsync(CancellationToken cancellationToken = default)
    {
        var workflows = await unitOfWork.Repository<Workflow>().ListAsync(cancellationToken: cancellationToken);
        var responses = new List<WorkflowResponse>(workflows.Count);

        foreach (var workflow in workflows)
        {
            var taskCount = await CountTasksAsync(workflow.Id, cancellationToken);
            responses.Add(Map(workflow, taskCount));
        }

        return responses;
    }

    public async Task<WorkflowResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var workflow = await unitOfWork.Repository<Workflow>().GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Workflow), id);
        var taskCount = await CountTasksAsync(id, cancellationToken);
        return Map(workflow, taskCount);
    }

    public async Task<WorkflowResponse> CreateAsync(CreateWorkflowRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = currentUserService.TenantId ?? throw new AuthenticationException("No authenticated tenant.");
        var userId = currentUserService.UserId ?? throw new AuthenticationException("No authenticated user.");

        var workflow = new Workflow
        {
            TenantId = tenantId,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Status = WorkflowStatus.Draft,
            CreatedByUserId = userId
        };

        await unitOfWork.Repository<Workflow>().AddAsync(workflow, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(workflow, 0);
    }

    public async Task<WorkflowResponse> UpdateAsync(Guid id, UpdateWorkflowRequest request, CancellationToken cancellationToken = default)
    {
        var workflows = unitOfWork.Repository<Workflow>();
        var workflow = await workflows.GetByIdAsync(id, cancellationToken) ?? throw new NotFoundException(nameof(Workflow), id);

        if (!Enum.TryParse<WorkflowStatus>(request.Status, ignoreCase: true, out var status))
        {
            throw new ConflictException($"'{request.Status}' is not a valid workflow status.");
        }

        workflow.Name = request.Name.Trim();
        workflow.Description = request.Description?.Trim();
        workflow.Status = status;
        workflow.UpdatedAtUtc = DateTime.UtcNow;

        workflows.Update(workflow);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var taskCount = await CountTasksAsync(id, cancellationToken);
        return Map(workflow, taskCount);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var workflows = unitOfWork.Repository<Workflow>();
        var workflow = await workflows.GetByIdAsync(id, cancellationToken) ?? throw new NotFoundException(nameof(Workflow), id);
        workflows.Remove(workflow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<int> CountTasksAsync(Guid workflowId, CancellationToken cancellationToken)
    {
        var tasks = await unitOfWork.Repository<WorkflowTask>().ListAsync(t => t.WorkflowId == workflowId, cancellationToken);
        return tasks.Count;
    }

    private static WorkflowResponse Map(Workflow workflow, int taskCount) => new(
        workflow.Id,
        workflow.Name,
        workflow.Description,
        workflow.Status.ToString(),
        workflow.CreatedByUserId,
        workflow.CreatedAtUtc,
        taskCount);
}
