using Fiap.CRM.Infra.CrossCutting.Http.Keycloak.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Fiap.CRM.Infra.CrossCutting.Http.Keycloak.Service
{
    public class KeycloakService : IKeycloakService
    {
        private readonly HttpClient _http;
        private readonly ILogger<KeycloakService> _logger;
        private readonly KeycloakOptions _options;

        private string? _accessToken;
        private DateTimeOffset _tokenExpiresAt = DateTimeOffset.MinValue;

        public KeycloakService(HttpClient http, ILogger<KeycloakService> logger, IOptions<KeycloakOptions> options)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

            if (string.IsNullOrEmpty(_options.AuthorityHost))
                throw new ArgumentNullException(nameof(_options.AuthorityHost), "AuthorityHost do Keycloak não pode ser nulo");
            if (string.IsNullOrEmpty(_options.Realm))
                throw new ArgumentNullException(nameof(_options.Realm), "Realm do Keycloak não pode ser nulo");
        }

        private async Task<string> GetAdminTokenAsync()
        {
            if (!string.IsNullOrEmpty(_accessToken) && _tokenExpiresAt > DateTimeOffset.UtcNow)
                return _accessToken;

            var tokenEndpoint = $"{_options.AuthorityHost}/realms/{_options.Realm}/protocol/openid-connect/token";

            var body = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", _options.Admin.ClientId),
                new KeyValuePair<string, string>("client_secret", _options.Admin.ClientSecret),
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            });

            var response = await _http.PostAsync(tokenEndpoint, body);
            response.EnsureSuccessStatusCode();

            var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            _accessToken = payload.RootElement.GetProperty("access_token").GetString();
            var expiresIn = payload.RootElement.GetProperty("expires_in").GetInt32();
            _tokenExpiresAt = DateTimeOffset.UtcNow.AddSeconds(expiresIn - 60);

            return _accessToken!;
        }

        private async Task<HttpRequestMessage> CreateRequestAsync(HttpMethod method, string url, object? content = null)
        {
            var token = await GetAdminTokenAsync();

            var request = new HttpRequestMessage(method, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            if (content != null)
            {
                request.Content = new StringContent(JsonSerializer.Serialize(content), Encoding.UTF8, "application/json");
            }

            return request;
        }

        public async Task<bool> CreateUserAsync(string email, string name, string password, string role)
        {
            var url = $"{_options.AuthorityHost}/admin/realms/{_options.Realm}/users";

            var userPayload = new
            {
                username = email,
                email,
                firstName = name,
                enabled = true,
                credentials = new[]
                {
                    new {
                        type = "password",
                        value = password,
                        temporary = false
                    }
                }
            };

            var request = await CreateRequestAsync(HttpMethod.Post, url, userPayload);
            var response = await _http.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Erro ao criar usuário {Email}: {Status}", email, response.StatusCode);
                return false;
            }

            var location = response.Headers.Location?.ToString();
            if (string.IsNullOrEmpty(location))
            {
                _logger.LogWarning("Usuário criado, mas Location header ausente. Role não será atribuída.");
                return true;
            }

            if (!string.IsNullOrEmpty(role))
            {
                var userId = location.Split('/').Last();
                var roleUrl = $"{_options.AuthorityHost}/admin/realms/{_options.Realm}/roles/{role}";
                var roleRequest = await CreateRequestAsync(HttpMethod.Get, roleUrl);
                var roleResponse = await _http.SendAsync(roleRequest);

                if (!roleResponse.IsSuccessStatusCode)
                {
                    _logger.LogError("Erro ao buscar role {Role}: {Status}", role, roleResponse.StatusCode);
                    return false;
                }

                var roleContent = await roleResponse.Content.ReadAsStringAsync();
                var roleObj = JsonSerializer.Deserialize<JsonElement>(roleContent);
                var roleRep = new[] { roleObj! };

                var roleMappingUrl = $"{_options.AuthorityHost}/admin/realms/{_options.Realm}/users/{userId}/role-mappings/realm";
                var assignRequest = await CreateRequestAsync(HttpMethod.Post, roleMappingUrl, roleRep);
                var assignResponse = await _http.SendAsync(assignRequest);

                if (!assignResponse.IsSuccessStatusCode)
                {
                    _logger.LogError("Erro ao atribuir role {Role} para usuário {Email}: {Status}", role, email, assignResponse.StatusCode);
                    return false;
                }
            }

            return true;
        }

        public async Task<bool> DeleteUserAsync(string email)
        {
            var url = $"{_options.AuthorityHost}/admin/realms/{_options.Realm}/users?email={email}";
            var request = await CreateRequestAsync(HttpMethod.Get, url);

            var response = await _http.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Erro ao buscar usuário {Email}: {Status}", email, response.StatusCode);
                return false;
            }

            var content = await response.Content.ReadAsStringAsync();
            var users = JsonSerializer.Deserialize<List<JsonElement>>(content);

            if (users == null || users.Count == 0)
            {
                _logger.LogWarning("Usuário com email {Email} não encontrado", email);
                return false;
            }

            var userId = users[0].GetProperty("id").GetString();

            var deleteUrl = $"{_options.AuthorityHost}/admin/realms/{_options.Realm}/users/{userId}";
            var deleteRequest = await CreateRequestAsync(HttpMethod.Delete, deleteUrl);

            var deleteResponse = await _http.SendAsync(deleteRequest);

            if (!deleteResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Erro ao deletar usuário {Email}: {Status}", email, deleteResponse.StatusCode);
                return false;
            }

            return true;
        }
    }
}
