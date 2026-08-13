using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexaFlow.Application.DTOs.Users;
using NexaFlow.Application.Interfaces;
using NexaFlow.Domain.Enums;

namespace NexaFlow.Api.Controllers;

[ApiController]
[Route("users")]
[Authorize]
public class UsersController(
    IUserService userService,
    IValidator<AdminCreateUserRequest> createValidator,
    IValidator<UpdateUserRequest> updateValidator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserSummaryResponse>>> List(CancellationToken cancellationToken) =>
        Ok(await userService.ListAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserSummaryResponse>> GetById(Guid id, CancellationToken cancellationToken) =>
        Ok(await userService.GetByIdAsync(id, cancellationToken));

    [HttpPost]
    [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Manager)}")]
    public async Task<ActionResult<UserSummaryResponse>> Create(AdminCreateUserRequest request, CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);
        var result = await userService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Manager)}")]
    public async Task<ActionResult<UserSummaryResponse>> Update(Guid id, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        await updateValidator.ValidateAndThrowAsync(request, cancellationToken);
        return Ok(await userService.UpdateAsync(id, request, cancellationToken));
    }
}
