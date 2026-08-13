using FluentValidation;
using NexaFlow.Application.DTOs.WorkflowTasks;

namespace NexaFlow.Application.Validators;

public sealed class CreateWorkflowTaskRequestValidator : AbstractValidator<CreateWorkflowTaskRequest>
{
    public CreateWorkflowTaskRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
    }
}
