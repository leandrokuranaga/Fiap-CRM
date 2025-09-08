using System.Net;
using Fiap.CRM.Api.SwaggerExamples.Common;
using Fiap.CRM.Application.Common;
using Fiap.CRM.Application.Users.Models.Request;
using Fiap.CRM.Application.Users.Models.Response;
using Fiap.CRM.Application.Users.Services;
using Fiap.CRM.Domain.SeedWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace Fiap.CRM.Api.Controllers;

/// <summary>
/// Controller used to manage all users
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "v1")]
[Route("api/v{version:apiVersion}/[controller]")]
public class UsersController(IUsersService usersService, INotification notification) : BaseController(notification)
{
    /// <summary>
    /// Creates a new regular user
    /// </summary>
    /// <param name="request">The details of the user to create.</param>
    /// <returns>The new user created information.</returns>
    [HttpPost("create")]
    [SwaggerOperation(
        Summary = "Creates a new regular user.",
        Description = "Registers a new user with basic access permissions. Returns the user data on success, or an error if the input is invalid or the email is already in use."
    )]
    [ProducesResponseType(typeof(SuccessResponse<UserResponse>), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status400BadRequest)]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(GenericErrorBadRequestExample))]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status409Conflict)]
    [SwaggerResponseExample(StatusCodes.Status409Conflict, typeof(GenericErrorConflictExample))]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(GenericErrorInternalServerExample))]
    public async Task<IActionResult> CreateAsync([FromBody] CreateUserRequest request)
    {
        var result = await usersService.CreateAsync(request);
        return Response<UserResponse>(result.UserId, result);
    }

    /// <summary>
    /// Creates a new user (admin only).
    /// </summary>
    /// <param name="request">The details of the user to create (only admin).</param>
    /// <returns>The new user created information.</returns>
    [HttpPost("create-admin")]
    [SwaggerOperation(
        Summary = "Creates a new user (admin only).",
        Description = "Creates a user account that can be either admin or regular. Requires admin privileges. Returns the created user's information or an error in case of validation failure, duplication, or unauthorized access."
    )]
    [ProducesResponseType(typeof(SuccessResponse<UserResponse>), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status400BadRequest)]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(GenericErrorBadRequestExample))]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status409Conflict)]
    [SwaggerResponseExample(StatusCodes.Status409Conflict, typeof(GenericErrorConflictExample))]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status401Unauthorized)]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(GenericErrorUnauthorizedExample))]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status403Forbidden)]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(GenericErrorForbiddenExample))]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(GenericErrorInternalServerExample))]
    public async Task<IActionResult> CreateAdminAsync([FromBody] CreateUserAdminRequest request)
    {
        var result = await usersService.CreateAdminAsync(request);
        return Response<UserResponse>(result.UserId, result);
    }

    /// <summary>
    /// Updates an existing user.
    /// </summary>
    /// <param name="request">The details of the user to be updated.</param>
    /// <param name="id">id of the updated user</param>
    /// <returns>The new user updated information.</returns>
    [HttpPatch("{id:int:min(1)}")]
    [SwaggerOperation(
        Summary = "Updates an existing user.",
        Description = "Updates the details of an existing user such as name, email, or status. Only accessible by admins. Returns 204 on success, 400 or 404 on failure, and 401/403 if unauthorized."
    )]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status400BadRequest)]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(GenericErrorBadRequestExample))]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status404NotFound)]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(GenericErrorNotFoundExample))]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status401Unauthorized)]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(GenericErrorUnauthorizedExample))]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status403Forbidden)]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(GenericErrorForbiddenExample))]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(GenericErrorInternalServerExample))]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateUserRequest request)
    {
        await usersService.UpdateAsync(id, request);
        return Response(BaseResponse<UserResponse>.Ok(null));
    }

    /// <summary>
    /// Deletes a user by ID.
    /// </summary>
    /// <param name="id">id of the deleted user</param>
    /// <returns>The deleted user information.</returns>
    [HttpDelete("{id:int:min(1)}")]
    [SwaggerOperation(
        Summary = "Deletes a user by ID.",
        Description = "Deletes a user from the system based on their ID. Only accessible by admins. Returns 204 on success, or an error if the user is not found or the requester lacks permission."
    )]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status400BadRequest)]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(GenericErrorBadRequestExample))]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status404NotFound)]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(GenericErrorNotFoundExample))]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status401Unauthorized)]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(GenericErrorUnauthorizedExample))]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status403Forbidden)]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(GenericErrorForbiddenExample))]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(GenericErrorInternalServerExample))]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        await usersService.DeleteAsync(id);
        return Response(BaseResponse<UserResponse>.Ok(null));
    }

    /// <summary>
    /// Retrieve a user by ID.
    /// </summary>
    /// <param name="id">Gets a user based on the id.</param>
    /// <returns>The user information.</returns>
    [HttpGet("{id:int:min(1)}")]
    [SwaggerOperation(
        Summary = "Retrieve a user by ID.",
        Description = "Fetches a user's details based on their ID. Only accessible by admins. Returns the user's information on success, or an error if not found or unauthorized."
    )]
    [ProducesResponseType(typeof(SuccessResponse<UserResponse>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status400BadRequest)]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(GenericErrorBadRequestExample))]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status401Unauthorized)]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(GenericErrorUnauthorizedExample))]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status403Forbidden)]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(GenericErrorForbiddenExample))]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(GenericErrorInternalServerExample))]
    public async Task<IActionResult> GetAsync(int id)
    {
        var result = await usersService.GetAsync(id);
        return Response(BaseResponse<UserResponse>.Ok(result));
    }

    /// <summary>
    /// Retrieve a list of all users.
    /// </summary>
    /// <returns>The list of users information.</returns>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Retrieve a list of all users.",
        Description = "Returns a list of all registered users. Only accessible by admins. May return an empty list if no users exist. Returns 401/403 if access is unauthorized."
    )]
    [ProducesResponseType(typeof(SuccessResponse<List<UserResponse>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status401Unauthorized)]
    [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(GenericErrorUnauthorizedExample))]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status403Forbidden)]
    [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(GenericErrorForbiddenExample))]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(GenericErrorInternalServerExample))]
    public async Task<IActionResult> GetAllAsync()
    {
        var result = await usersService.GetAllAsync();
        return Response(BaseResponse<List<UserResponse>>.Ok(result));
    }
}