using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fiap.CRM.Infra.CrossCutting.Http.Keycloak.Models
{
    public class KeycloakOptions
    {
        public string Authority { get; set; } = null!;
        public string AuthorityHost { get; set; } = null!;
        public string Realm { get; set; } = null!;
        public AdminOptions Admin { get; set; } = new AdminOptions();

        public class AdminOptions
        {
            public string ClientId { get; set; } = null!;
            public string ClientSecret { get; set; } = null!;
        }
    }
}
