using Fiap.CRM.Infra.CrossCutting.Http.Keycloak.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fiap.CRM.Infra.CrossCutting.Http.Keycloak.Service
{
    public interface IKeycloakService
    {
        Task<bool> CreateUserAsync(string email, string name, string lastName, string password, string role);
        Task<bool> DeleteUserAsync(string email);
        Task<KeycloakTokenResponse?> GetUserTokenAsync(string username, string password);
        Task<bool> UpdateUserAsync(string oldEmail, string newEmail, string newName, string? newPassword);
    }
}
