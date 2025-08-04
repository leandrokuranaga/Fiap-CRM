using System.Diagnostics.CodeAnalysis;
using Fiap.CRM.Application.Users.Models.Request;
using Fiap.CRM.Domain.UserAggregate.Enums;

namespace Fiap.CRM.Api.SwaggerExamples.Users;

[ExcludeFromCodeCoverage]
public class CreateUserAdminRequestExample : IExamplesProvider<CreateUserAdminRequest>
{
    public CreateUserAdminRequest GetExamples()
    {
        return new CreateUserAdminRequest
        {
            Name = "John Doe",
            Email = "john.doe@hotmail.com",
            Password = "Password123!",
            TypeUser = TypeUser.Admin,
            Active = true
        };
    }
}