using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexaFlow.Application.DTOs.WorkflowTasks;
using NexaFlow.Application.Interfaces;
using NexaFlow.Domain.Enums;

namespace NexaFlow.Api.Controllers;

[ApiController]
[Route("workflows/{workflowId:guid}/tasks")]
[Authorize]
public class WorkflowTasksController(
    IWorkflowTaskService taskService,
    IValidator<CreateWorkflowTaskRequest> createValidator,
    IValidator<UpdateWorkflowTaskRequest> updateValidator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<WorkflowTaskResponse>>> List(Guid workflowId, CancellationToken cancellationToken) =>
        Ok(await taskService.ListByWorkflowAsync(workflowId, cancellationToken));

    [HttpGet("{taskId:guid}")]
    public async Task<ActionResult<WorkflowTaskResponse>> GetById(Guid workflowId, Guid taskId, CancellationToken cancellationToken) =>
        Ok(await taskService.GetByIdAsync(workflowId, taskId, cancellationToken));

    [HttpPost]
    [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Manager)}")]
    public async Task<ActionResult<WorkflowTaskResponse>> Create(Guid workflowId, CreateWorkflowTaskRequest request, CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);
        var result = await taskService.CreateAsync(workflowId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { workflowId, taskId = result.Id }, result);
    }

    /// <summary>Any authenticated tenant member can update task status — typically the assignee.</summary>
    [HttpPut("{taskId:guid}")]
    public async Task<ActionResult<WorkflowTaskResponse>> Update(Guid workflowId, Guid taskId, UpdateWorkflowTaskRequest request, CancellationToken cancellationToken)
    {
        await updateValidator.ValidateAndThrowAsync(request, cancellationToken);
        return Ok(await taskService.UpdateAsync(workflowId, taskId, request, cancellationToken));
    }

    [HttpDelete("{taskId:guid}")]
    [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Manager)}")]
    public async Task<IActionResult> Delete(Guid workflowId, Guid taskId, CancellationToken cancellationToken)
    {
        await taskService.DeleteAsync(workflowId, taskId, cancellationToken);
        return NoContent();
    }
}
