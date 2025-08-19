using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fiap.CRM.Infra.Utils
{
    public class KeycloakSettings
    {
        public const string SectionName = "Keycloak";

        public string Authority { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
    }
}
