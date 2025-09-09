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
                throw new ArgumentNullException(nameof(_options.AuthorityHost), "Keycloak's AuthorityHost cannot be null");
            if (string.IsNullOrEmpty(_options.Realm))
                throw new ArgumentNullException(nameof(_options.Realm), "Keycloak's Realm cannot be null");
            if (string.IsNullOrEmpty(_options.ClientId))
                throw new ArgumentNullException(nameof(_options.ClientId), "Keycloak ClientId cannot be null");
            if (string.IsNullOrEmpty(_options.ClientSecret))
                throw new ArgumentNullException(nameof(_options.ClientSecret), "Keycloak's ClientSecret cannot be null");
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

        public async Task<KeycloakTokenResponse?> GetUserTokenAsync(string username, string password)
        {
            var tokenEndpoint = $"{_options.AuthorityHost}/realms/{_options.Realm}/protocol/openid-connect/token";

            var body = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", _options.ClientId),
                new KeyValuePair<string, string>("client_secret", _options.ClientSecret),
                new KeyValuePair<string, string>("username", username),
                new KeyValuePair<string, string>("password", password),
                new KeyValuePair<string, string>("grant_type", "password")
            });

            try
            {
                var response = await _http.PostAsync(tokenEndpoint, body);
                response.EnsureSuccessStatusCode();

                var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                return JsonSerializer.Deserialize<KeycloakTokenResponse>(payload);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to get user token {Username}. Status: {Status}", username, ex.StatusCode);
                return null;
            }
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

        public async Task<bool> CreateUserAsync(string email, string name, string lastName, string password, string role)
        {
            
            var url = $"{_options.AuthorityHost}/admin/realms/{_options.Realm}/users";

            var userPayload = new
            {
                username = email,
                email,
                firstName = name,
                lastName = lastName,
                enabled = true,
                emailVerified = true,
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
                _logger.LogError("Error creating user {Email}: {Status}", email, response.StatusCode);
                return false;
            }

            var location = response.Headers.Location?.ToString();
            if (string.IsNullOrEmpty(location))
            {
                _logger.LogWarning("User created, but Location header missing. Role will not be assigned.");
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
                    _logger.LogError("Error when searching for role {Role}: {Status}", role, roleResponse.StatusCode);
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
                    _logger.LogError("Error assigning role {Role} to user {Email}: {Status}", role, email, assignResponse.StatusCode);
                    return false;
                }
            }

            return true;
        }

        public async Task<bool> UpdateUserAsync(string oldEmail, string newEmail, string newName, string? newPassword)
        {
           
            var url = $"{_options.AuthorityHost}/admin/realms/{_options.Realm}/users?email={oldEmail}";
            var request = await CreateRequestAsync(HttpMethod.Get, url);

            var response = await _http.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Error when searching for user {Email}: {Status}", oldEmail, response.StatusCode);
                return false;
            }

            var content = await response.Content.ReadAsStringAsync();
            var users = JsonSerializer.Deserialize<List<JsonElement>>(content);

            if (users is null || users.Count == 0)
            {
                _logger.LogWarning("User with email {Email} not found", oldEmail);
                return false;
            }

            var userId = users[0].GetProperty("id").GetString();

           
            var userUpdatePayload = new
            {
                username = newEmail,
                email = newEmail,
                firstName = newName,
                enabled = true 
            };

            var updateUrl = $"{_options.AuthorityHost}/admin/realms/{_options.Realm}/users/{userId}";
            var updateRequest = await CreateRequestAsync(HttpMethod.Put, updateUrl, userUpdatePayload);

            var updateResponse = await _http.SendAsync(updateRequest);
            if (!updateResponse.IsSuccessStatusCode)
            {
                var errorContent = await updateResponse.Content.ReadAsStringAsync();
                _logger.LogError("Erro atualizando usuário: {StatusCode}. Detalhes da API do Keycloak: {ErrorContent}", updateResponse.StatusCode, errorContent);
                return false;
            }

            if (!string.IsNullOrEmpty(newPassword))
            {
                var passwordPayload = new
                {
                    type = "password",
                    value = newPassword,
                    temporary = false
                };

                var passwordUrl = $"{_options.AuthorityHost}/admin/realms/{_options.Realm}/users/{userId}/reset-password";
                var passwordRequest = await CreateRequestAsync(HttpMethod.Put, passwordUrl, passwordPayload);

                var passwordResponse = await _http.SendAsync(passwordRequest);
                if (!passwordResponse.IsSuccessStatusCode)
                {
                    _logger.LogError("Error updating user password for {Email}: {Status}", newEmail, passwordResponse.StatusCode);
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
                _logger.LogError("Error when searching for user {Email}: {Status}", email, response.StatusCode);
                return false;
            }

            var content = await response.Content.ReadAsStringAsync();
            var users = JsonSerializer.Deserialize<List<JsonElement>>(content);

            if (users is null || users.Count == 0)
            {
                _logger.LogWarning("User with email {Email} not found", email);
                return false;
            }

            var userId = users[0].GetProperty("id").GetString();

            var deleteUrl = $"{_options.AuthorityHost}/admin/realms/{_options.Realm}/users/{userId}";
            var deleteRequest = await CreateRequestAsync(HttpMethod.Delete, deleteUrl);

            var deleteResponse = await _http.SendAsync(deleteRequest);

            if (!deleteResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Error deleting user {Email}: {Status}", email, deleteResponse.StatusCode);
                return false;
            }

            return true;
        }
    }
}
