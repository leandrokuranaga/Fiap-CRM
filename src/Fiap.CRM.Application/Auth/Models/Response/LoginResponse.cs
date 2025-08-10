namespace Fiap.CRM.Application.Auth.Models.Response;

public class LoginResponse
{
    public string Token { get; set; }
    public DateTime Expiration { get; set; }
}