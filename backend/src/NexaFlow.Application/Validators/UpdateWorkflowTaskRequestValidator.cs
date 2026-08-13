using FluentValidation;
using NexaFlow.Application.DTOs.WorkflowTasks;
using NexaFlow.Domain.Enums;

namespace NexaFlow.Application.Validators;

public sealed class UpdateWorkflowTaskRequestValidator : AbstractValidator<UpdateWorkflowTaskRequest>
{
    public UpdateWorkflowTaskRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Status).NotEmpty().Must(s => Enum.TryParse<WorkflowTaskStatus>(s, ignoreCase: true, out _))
            .WithMessage("Status must be one of: Pending, InProgress, Blocked, Completed, Cancelled.");
    }
}
