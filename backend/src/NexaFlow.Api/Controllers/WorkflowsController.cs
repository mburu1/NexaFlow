using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexaFlow.Application.DTOs.Workflows;
using NexaFlow.Application.Interfaces;
using NexaFlow.Domain.Enums;

namespace NexaFlow.Api.Controllers;

[ApiController]
[Route("workflows")]
[Authorize]
public class WorkflowsController(
    IWorkflowService workflowService,
    IValidator<CreateWorkflowRequest> createValidator,
    IValidator<UpdateWorkflowRequest> updateValidator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<WorkflowResponse>>> List(CancellationToken cancellationToken) =>
        Ok(await workflowService.ListAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WorkflowResponse>> GetById(Guid id, CancellationToken cancellationToken) =>
        Ok(await workflowService.GetByIdAsync(id, cancellationToken));

    [HttpPost]
    [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Manager)}")]
    public async Task<ActionResult<WorkflowResponse>> Create(CreateWorkflowRequest request, CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);
        var result = await workflowService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Manager)}")]
    public async Task<ActionResult<WorkflowResponse>> Update(Guid id, UpdateWorkflowRequest request, CancellationToken cancellationToken)
    {
        await updateValidator.ValidateAndThrowAsync(request, cancellationToken);
        return Ok(await workflowService.UpdateAsync(id, request, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await workflowService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
