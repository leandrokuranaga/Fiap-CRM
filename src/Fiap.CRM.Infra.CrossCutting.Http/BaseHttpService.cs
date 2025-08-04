using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Newtonsoft.Json;

namespace Fiap.CRM.Infra.CrossCutting.Http;

[ExcludeFromCodeCoverage]
public abstract class BaseHttpService(HttpClient httpClient)
{
    protected async Task<T> GetAsync<T>(string url, NameValueCollection? queryParams) where T : new()
    {
        if (queryParams is not null)
        {
            url = $"{url}?{queryParams}";
        }

        var response = await httpClient.GetAsync(url);

        response.EnsureSuccessStatusCode();

        return await DeserializeResponse<T>(response);
    }

    protected async Task<T> GetAsync<T>(string url) where T : new()
    {
        return await GetAsync<T>(url, null);
    }

    protected async Task<T> PostAsync<T>(string url, object request) where T : new()
    {
        var json = JsonConvert.SerializeObject(request);
        var body = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(url, body);

        response.EnsureSuccessStatusCode();

        return await DeserializeResponse<T>(response);
    }

    private static async Task<T> DeserializeResponse<T>(HttpResponseMessage response)
    {
        var responseMessage = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<T>(responseMessage)!;
    }
}