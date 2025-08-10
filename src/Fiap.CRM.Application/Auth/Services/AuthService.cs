using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Fiap.CRM.Application.Auth.Models.Request;
using Fiap.CRM.Application.Auth.Models.Response;
using Fiap.CRM.Application.Common;
using Fiap.CRM.Application.Validators.AuthValidators;
using Fiap.CRM.Domain.SeedWork;
using Fiap.CRM.Domain.UserAggregate;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Fiap.CRM.Application.Auth.Services;

public class AuthService(
        IUserRepository userRepository,
        INotification notification,
        IConfiguration configuration
        ) : BaseService(notification), IAuthService
    {
        public async Task<LoginResponse> LoginAsync(LoginRequest request) => await ExecuteAsync(async () =>
        {
            Validate(request, new LoginRequestValidator());

            var login = new LoginResponse();

            var username = request.Username.Trim().ToLowerInvariant();

            var user = await userRepository.GetOneNoTracking(a => a.Email.Address.ToLower() == username);

            if (user is null)
            {
                notification.AddNotification("Login Failed", "Invalid username or password.", NotificationModel.ENotificationType.NotFound);
                return login;
            }

            if (!user.Active)
            {
                notification.AddNotification("Login Failed", "Your account is disabled. Please contact support.", NotificationModel.ENotificationType.BusinessRules);
                return login;
            }

            bool validPassword = user.Password.Challenge(request.Password, user.Password.PasswordSalt);

            if (!validPassword)
            {
                notification.AddNotification("Login Failed", "Invalid username or password.", NotificationModel.ENotificationType.BusinessRules);
                return login;
            }
            var (Token, Expiration) = GenerateJwtToken(user);

            return new LoginResponse
            {
                Token = Token,
                Expiration = Expiration
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