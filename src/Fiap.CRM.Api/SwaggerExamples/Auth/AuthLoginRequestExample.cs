using System.Diagnostics.CodeAnalysis;
using Fiap.CRM.Application.Auth.Models.Request;
using Swashbuckle.AspNetCore.Filters;

namespace Fiap.CRM.Api.SwaggerExamples.Auth;

[ExcludeFromCodeCoverage]
public class AuthLoginRequestExample : IExamplesProvider<LoginRequest>
{
    public LoginRequest GetExamples()
    {
        return new LoginRequest
        {
            Username = "john.doe@hotmail.com",
            Password = "Password123!"
        };
    }
}