using FluentValidation;
using NexaFlow.Application.DTOs.Workflows;

namespace NexaFlow.Application.Validators;

public sealed class CreateWorkflowRequestValidator : AbstractValidator<CreateWorkflowRequest>
{
    public CreateWorkflowRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
    }
}
