using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexaFlow.Application.DTOs.Tenants;
using NexaFlow.Application.Interfaces;
using NexaFlow.Domain.Enums;

namespace NexaFlow.Api.Controllers;

[ApiController]
[Route("tenants/current")]
[Authorize]
public class TenantsController(ITenantService tenantService, IValidator<UpdateTenantRequest> updateValidator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<TenantResponse>> GetCurrent(CancellationToken cancellationToken) =>
        Ok(await tenantService.GetCurrentAsync(cancellationToken));

    [HttpPut]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<ActionResult<TenantResponse>> UpdateCurrent(UpdateTenantRequest request, CancellationToken cancellationToken)
    {
        await updateValidator.ValidateAndThrowAsync(request, cancellationToken);
        return Ok(await tenantService.UpdateCurrentAsync(request, cancellationToken));
    }
}
