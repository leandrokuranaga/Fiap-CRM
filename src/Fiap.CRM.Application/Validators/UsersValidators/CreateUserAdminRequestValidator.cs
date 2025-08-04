using System.Diagnostics.CodeAnalysis;
using Fiap.CRM.Application.Users.Models.Request;

namespace Fiap.CRM.Application.Validators.UsersValidators;

[ExcludeFromCodeCoverage]
public class CreateUserAdminRequestValidator : AbstractValidator<CreateUserAdminRequest>
{
    public CreateUserAdminRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required.")
            .MaximumLength(100)
            .WithMessage("Name must be less than or equal to 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Email must be a valid email address.")
            .MaximumLength(100)
            .WithMessage("Email must be less than or equal to 100 characters.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required.")
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters long.")
            .MaximumLength(100)
            .WithMessage("Password must be less than or equal to 100 characters.")
            .Matches(@"^(?=.*[a-zA-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$")
            .WithMessage("Password must contain at least one letter, one number, and one special character.");

        RuleFor(x => x.TypeUser)
            .IsInEnum().WithMessage("TypeUser must be a valid value (e.g., Admin, User).");

        RuleFor(x => x.Active)
            .NotNull().WithMessage("Active status is required.");
    }
}