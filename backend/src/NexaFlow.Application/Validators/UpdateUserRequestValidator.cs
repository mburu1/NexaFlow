using FluentValidation;
using NexaFlow.Application.DTOs.Users;
using NexaFlow.Domain.Enums;

namespace NexaFlow.Application.Validators;

public sealed class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Role).NotEmpty().Must(r => Enum.TryParse<Role>(r, ignoreCase: true, out _))
            .WithMessage("Role must be one of: Admin, Manager, Member.");
    }
}
