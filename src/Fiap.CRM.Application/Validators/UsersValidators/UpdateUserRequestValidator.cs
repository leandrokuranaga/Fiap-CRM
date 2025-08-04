using System.Diagnostics.CodeAnalysis;
using Fiap.CRM.Application.Users.Models.Request;

namespace Fiap.CRM.Application.Validators.UsersValidators;

[ExcludeFromCodeCoverage]
public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100)
            .WithMessage("Name must be less than or equal to 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.Email)
            .EmailAddress()
            .WithMessage("Email must be a valid email address.")
            .MaximumLength(100)
            .WithMessage("Email must be less than or equal to 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required.")
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters long.")
            .MaximumLength(100)
            .WithMessage("Password must be less than or equal to 100 characters.")
            .Matches(@"^(?=.*[a-zA-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$")
            .WithMessage("Password must contain at least one letter, one number, and one special character.")
            .When(x => !string.IsNullOrEmpty(x.Password));

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Type must be a valid enum value.")
            .When(x => x.Type.HasValue);

        RuleFor(x => x.Active)
            .NotNull()
            .WithMessage("Active status is required.")
            .When(x => x.Active.HasValue);

        RuleFor(x => x)
            .Must(x =>
                !string.IsNullOrWhiteSpace(x.Name) ||
                !string.IsNullOrWhiteSpace(x.Email) ||
                !string.IsNullOrWhiteSpace(x.Password) ||
                x.Type.HasValue ||
                x.Active.HasValue
            )
            .WithMessage("At least one field must be provided.");
    }
}