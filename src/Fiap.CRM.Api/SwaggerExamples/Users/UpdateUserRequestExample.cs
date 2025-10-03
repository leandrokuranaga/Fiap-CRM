using System.Diagnostics.CodeAnalysis;
using Fiap.CRM.Application.Users.Models.Request;
using Fiap.CRM.Domain.UserAggregate.Enums;
using Swashbuckle.AspNetCore.Filters;

namespace Fiap.CRM.Api.SwaggerExamples.Users;

[ExcludeFromCodeCoverage]
public class UpdateUserRequestExample : IExamplesProvider<UpdateUserRequest>
{
    public UpdateUserRequest GetExamples()
    {
        return new UpdateUserRequest
        {
            Name = "Maria Carie",
            Email = "maria.carie@hotmail.com",
            Password = "Password456!",
            Type = TypeUser.Admin,
            Active = false
        };
    }
}