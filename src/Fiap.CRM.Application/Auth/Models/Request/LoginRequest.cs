namespace Fiap.CRM.Application.Auth.Models.Request;

public record LoginRequest
{
    public string Username { get; init; }
    public string Password { get; init; }
}