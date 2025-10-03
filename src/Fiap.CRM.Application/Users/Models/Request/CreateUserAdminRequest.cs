using System.Diagnostics.CodeAnalysis;
using Fiap.CRM.Domain.UserAggregate.Enums;

namespace Fiap.CRM.Application.Users.Models.Request;

[ExcludeFromCodeCoverage]
public record CreateUserAdminRequest
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public TypeUser TypeUser { get; set; }
    public bool Active { get; set; }

    public static explicit operator Domain.UserAggregate.User(CreateUserAdminRequest request)
    {
        return Domain.UserAggregate.User.CreateByAdmin(
            request.Name,
            request.Email,
            request.Password,
            request.TypeUser,
            request.Active
        );
    }
}