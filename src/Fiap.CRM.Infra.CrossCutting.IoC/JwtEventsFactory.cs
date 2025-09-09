using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

public static class JwtEventsFactory
{
    public static JwtBearerEvents CreateJwtBearerEvents(string audience)
    {
        return new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                if (context.Principal?.Identity is ClaimsIdentity claimsIdentity)
                {
                    // Extrai as roles do "realm_access"
                    if (context.Principal.FindFirst("realm_access") is Claim realmAccessClaim)
                    {
                        ExtractAndAddRoles(realmAccessClaim, claimsIdentity, "roles");
                    }

                    // Extrai as roles do "resource_access"
                    if (context.Principal.FindFirst("resource_access") is Claim resourceAccessClaim)
                    {
                        ExtractAndAddRoles(resourceAccessClaim, claimsIdentity, audience);
                    }
                }
                return Task.CompletedTask;
            }
        };
    }

    private static void ExtractAndAddRoles(Claim claim, ClaimsIdentity claimsIdentity, string key)
    {
        if (!string.IsNullOrEmpty(claim.Value) && claim.Value.Length > 0)
        {
            var jsonDocument = JsonDocument.Parse(claim.Value);
            if (jsonDocument.RootElement.TryGetProperty(key, out var rolesElement) && rolesElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var role in rolesElement.EnumerateArray())
                {
                    var roleName = role.GetString();
                    if (!string.IsNullOrEmpty(roleName))
                    {
                        claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, roleName));
                    }
                }
            }
        }
    }
}