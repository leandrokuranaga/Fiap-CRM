using Fiap.CRM.Infra.CrossCutting.Http.Keycloak.Models;

namespace Fiap.CRM.Infra.CrossCutting.Http.Keycloak.Service
{
    public interface IKeycloakService
    {
        Task<bool> CreateUserAsync(string email, string name, string password, string role);
        Task<bool> DeleteUserAsync(string email);
        Task<KeycloakTokenResponse?> GetUserTokenAsync(string username, string password);
    }
}
