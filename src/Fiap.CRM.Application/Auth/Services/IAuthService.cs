using Fiap.CRM.Application.Auth.Models.Request;
using Fiap.CRM.Application.Auth.Models.Response;

namespace Fiap.CRM.Application.Auth.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
}