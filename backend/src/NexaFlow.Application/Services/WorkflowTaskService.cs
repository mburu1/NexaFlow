using NexaFlow.Application.Common.Exceptions;
using NexaFlow.Application.Common.Interfaces;
using NexaFlow.Application.DTOs.WorkflowTasks;
using NexaFlow.Application.Interfaces;
using NexaFlow.Domain.Entities;
using NexaFlow.Domain.Enums;
using NexaFlow.Domain.Interfaces;

namespace NexaFlow.Application.Services;

public sealed class WorkflowTaskService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService) : IWorkflowTaskService
{
    public async Task<IReadOnlyList<WorkflowTaskResponse>> ListByWorkflowAsync(Guid workflowId, CancellationToken cancellationToken = default)
    {
        await EnsureWorkflowExistsAsync(workflowId, cancellationToken);
        var tasks = await unitOfWork.Repository<WorkflowTask>().ListAsync(t => t.WorkflowId == workflowId, cancellationToken);
        return tasks.Select(Map).ToList();
    }

    public async Task<WorkflowTaskResponse> GetByIdAsync(Guid workflowId, Guid taskId, CancellationToken cancellationToken = default)
    {
        var task = await GetOwnedTaskAsync(workflowId, taskId, cancellationToken);
        return Map(task);
    }

    public async Task<WorkflowTaskResponse> CreateAsync(Guid workflowId, CreateWorkflowTaskRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = currentUserService.TenantId ?? throw new AuthenticationException("No authenticated tenant.");
        await EnsureWorkflowExistsAsync(workflowId, cancellationToken);

        var task = new WorkflowTask
        {
            TenantId = tenantId,
            WorkflowId = workflowId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Status = WorkflowTaskStatus.Pending,
            AssignedToUserId = request.AssignedToUserId,
            DueAtUtc = request.DueAtUtc
        };

        await unitOfWork.Repository<WorkflowTask>().AddAsync(task, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(task);
    }

    public async Task<WorkflowTaskResponse> UpdateAsync(Guid workflowId, Guid taskId, UpdateWorkflowTaskRequest request, CancellationToken cancellationToken = default)
    {
        var tasks = unitOfWork.Repository<WorkflowTask>();
        var task = await GetOwnedTaskAsync(workflowId, taskId, cancellationToken);

        if (!Enum.TryParse<WorkflowTaskStatus>(request.Status, ignoreCase: true, out var status))
        {
            throw new ConflictException($"'{request.Status}' is not a valid task status.");
        }

        task.Title = request.Title.Trim();
        task.Description = request.Description?.Trim();
        task.Status = status;
        task.AssignedToUserId = request.AssignedToUserId;
        task.DueAtUtc = request.DueAtUtc;
        task.UpdatedAtUtc = DateTime.UtcNow;

        tasks.Update(task);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(task);
    }

    public async Task DeleteAsync(Guid workflowId, Guid taskId, CancellationToken cancellationToken = default)
    {
        var tasks = unitOfWork.Repository<WorkflowTask>();
        var task = await GetOwnedTaskAsync(workflowId, taskId, cancellationToken);
        tasks.Remove(task);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureWorkflowExistsAsync(Guid workflowId, CancellationToken cancellationToken)
    {
        _ = await unitOfWork.Repository<Workflow>().GetByIdAsync(workflowId, cancellationToken)
            ?? throw new NotFoundException(nameof(Workflow), workflowId);
    }

    private async Task<WorkflowTask> GetOwnedTaskAsync(Guid workflowId, Guid taskId, CancellationToken cancellationToken)
    {
        var task = await unitOfWork.Repository<WorkflowTask>().GetByIdAsync(taskId, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkflowTask), taskId);

        if (task.WorkflowId != workflowId)
        {
            throw new NotFoundException(nameof(WorkflowTask), taskId);
        }

        return task;
    }

    private static WorkflowTaskResponse Map(WorkflowTask task) => new(
        task.Id,
        task.WorkflowId,
        task.Title,
        task.Description,
        task.Status.ToString(),
        task.AssignedToUserId,
        task.DueAtUtc,
        task.CreatedAtUtc);
}
