namespace Fiap.CRM.Infra.CrossCutting.Keycloak
{
    public class KeycloakOptions
    {
        public string Authority { get; set; } = null!;
        public string AuthorityHost { get; set; } = null!;
        public string Realm { get; set; } = null!;
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
        public AdminOptions Admin { get; set; } = new AdminOptions();

        public class AdminOptions
        {
            public string ClientId { get; set; } = null!;
            public string ClientSecret { get; set; } = null!;
        }
    }
}