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

    private (string Token, DateTime Expiration) GenerateJwtToken(User user)
        {
            var secretKey = configuration["JwtSettings:SecretKey"];
            var key = Encoding.ASCII.GetBytes(secretKey);

            var claims = new List<Claim>
            {
                new("id", user.Id.ToString()),
                new("username", user.Email.Address),
                new(ClaimTypes.Role, user.TypeUser.ToString())
            };

            var expiration = DateTime.UtcNow.AddHours(2);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiration,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            return (tokenString, expiration);
        }
    }