using System.Net;
using Fiap.CRM.Api.SwaggerExamples.Common;
using Fiap.CRM.Application.Auth.Models.Request;
using Fiap.CRM.Application.Auth.Models.Response;
using Fiap.CRM.Application.Auth.Services;
using Fiap.CRM.Application.Common;
using Fiap.CRM.Domain.SeedWork;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace Fiap.CRM.Api.Controllers;

/// <summary>
/// Controller responsible for authentication operations such as user login.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "v1")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController(IAuthService authService, INotification notification) : BaseController(notification)
{
    /// <summary>
    /// Authenticates a user and generates a JWT token upon successful login.
    /// </summary>
    /// <param name="request">The login request containing username and password.</param>
    /// <returns>Returns a JWT token if authentication is successful; otherwise, returns an error.</returns>
    [HttpPost("login")]
    [SwaggerOperation(
        Summary = "Authenticates a user and generates a JWT token upon successful login.",
        Description = "This endpoint verifies the user's credentials. If valid, it returns a signed JWT token that can be used to authorize further requests. " +
                      "If the credentials are invalid, a 400 or 404 error is returned depending on the nature of the failure."
    )]
    [ProducesResponseType(typeof(SuccessResponse<LoginResponse>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status400BadRequest)]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(GenericErrorBadRequestExample))]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status404NotFound)]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(GenericErrorNotFoundExample))]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(GenericErrorInternalServerExample))]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request)
    {
        var result = await authService.LoginAsync(request);

        return Response(BaseResponse<LoginResponse>.Ok(result));
    }
}