namespace Fiap.CRM.Infra.CrossCutting.Keycloak
{
    public interface IKeycloakService
    {
        Task<bool> CreateUserAsync(string email, string name, string lastName, string password, string role);
        Task<bool> DeleteUserAsync(string email);
        Task<KeycloakTokenResponse?> GetUserTokenAsync(string username, string password);
        Task<bool> UpdateUserAsync(string oldEmail, string newEmail, string newName, string? newPassword);
    }
}