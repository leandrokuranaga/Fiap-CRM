using Fiap.CRM.Application.Auth.Models.Request;
using Fiap.CRM.Application.Auth.Models.Response;
using Fiap.CRM.Application.Common;
using Fiap.CRM.Application.Validators.AuthValidators;
using Fiap.CRM.Domain.SeedWork;
using Fiap.CRM.Domain.UserAggregate;
using Fiap.CRM.Infra.CrossCutting.Http.Keycloak.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Fiap.CRM.Application.Auth.Services;

public class AuthService(
        IUserRepository userRepository,
        INotification notification,
        IConfiguration configuration,
        IKeycloakService keycloakService
        ) : BaseService(notification), IAuthService
{
    public async Task<LoginResponse> LoginAsync(LoginRequest request) => await ExecuteAsync(async () =>
    {
        Validate(request, new LoginRequestValidator());

        var tokenResponse = await keycloakService.GetUserTokenAsync(request.Username, request.Password);

        if (tokenResponse is null)
        {
            notification.AddNotification("Login Failed", "Invalid username or password.", NotificationModel.ENotificationType.NotFound);
            return new LoginResponse();
        }

        var user = await userRepository.GetOneNoTracking(a => a.Email.Address.ToLower() == request.Username.Trim().ToLowerInvariant());

        if (user is null)
        {
            notification.AddNotification("Login Failed", "User not found in local database.", NotificationModel.ENotificationType.NotFound);
            return new LoginResponse();
        }

        if (!user.Active)
        {
            notification.AddNotification("Login Failed", "Your account is disabled. Please contact support.", NotificationModel.ENotificationType.BusinessRules);
            return new LoginResponse();
        }

        return new LoginResponse
        {
            Token = tokenResponse.AccessToken,
            Expiration = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn)
        };
    });   
}