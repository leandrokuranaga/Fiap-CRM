using System.Diagnostics.CodeAnalysis;
using Fiap.CRM.Application.Users.Models.Request;

namespace Fiap.CRM.Api.SwaggerExamples.Users;

[ExcludeFromCodeCoverage]
public class CreateUserRequestExample : IExamplesProvider<CreateUserRequest>
{
    public CreateUserRequest GetExamples()
    {
        return new CreateUserRequest
        {
            Name = "John Doe",
            Email = "john.doe@hotmail.com",
            Password = "Password123!"
        };
    }
}