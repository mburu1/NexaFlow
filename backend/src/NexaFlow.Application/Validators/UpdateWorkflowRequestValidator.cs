using FluentValidation;
using NexaFlow.Application.DTOs.Workflows;
using NexaFlow.Domain.Enums;

namespace NexaFlow.Application.Validators;

public sealed class UpdateWorkflowRequestValidator : AbstractValidator<UpdateWorkflowRequest>
{
    public UpdateWorkflowRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Status).NotEmpty().Must(s => Enum.TryParse<WorkflowStatus>(s, ignoreCase: true, out _))
            .WithMessage("Status must be one of: Draft, Active, Paused, Completed, Archived.");
    }
}
