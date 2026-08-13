using FluentValidation;
using NexaFlow.Application.DTOs.Tenants;

namespace NexaFlow.Application.Validators;

public sealed class UpdateTenantRequestValidator : AbstractValidator<UpdateTenantRequest>
{
    public UpdateTenantRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}
