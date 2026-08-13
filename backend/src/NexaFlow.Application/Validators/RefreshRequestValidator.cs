using FluentValidation;
using NexaFlow.Application.DTOs.Auth;

namespace NexaFlow.Application.Validators;

public sealed class RefreshRequestValidator : AbstractValidator<RefreshRequest>
{
    public RefreshRequestValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}
