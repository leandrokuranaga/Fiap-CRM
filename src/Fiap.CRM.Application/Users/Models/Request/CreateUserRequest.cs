using System.Diagnostics.CodeAnalysis;

namespace Fiap.CRM.Application.Users.Models.Request;

[ExcludeFromCodeCoverage]
public record CreateUserRequest
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }

    public static explicit operator Domain.UserAggregate.User(CreateUserRequest request)
    {
        return Domain.UserAggregate.User.CreateByPublic(
            request.Name,
            request.Email,
            request.Password
        );
    }
}