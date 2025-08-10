using System.Diagnostics.CodeAnalysis;
using Fiap.CRM.Application.Users.Models.Request;
using Swashbuckle.AspNetCore.Filters;

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