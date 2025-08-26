using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fiap.CRM.Infra.CrossCutting.Http.Keycloak.Service
{
    public interface IKeycloakService
    {
        Task<bool> CreateUserAsync(string email, string name, string password, string role);
        Task<bool> DeleteUserAsync(string email);
    }
}
