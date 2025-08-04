using System.Diagnostics.CodeAnalysis;
using Fiap.CRM.Application.Auth.Models.Request;

namespace Fiap.CRM.Application.Validators.AuthValidators;

[ExcludeFromCodeCoverage]
public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Username is required.")
            .EmailAddress()
            .WithMessage("Username must be a valid email address.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");           
    }
}