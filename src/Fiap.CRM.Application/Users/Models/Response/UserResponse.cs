using Fiap.CRM.Domain.UserAggregate;
using Fiap.CRM.Domain.UserAggregate.Enums;
using System.Diagnostics.CodeAnalysis;

namespace Fiap.CRM.Application.Users.Models.Response;

[ExcludeFromCodeCoverage]
public record UserResponse
{
    public int UserId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public TypeUser Type { get; set; }
    public bool Active { get; set; }
    public List<int> GameIds { get; set; } = [];

    public static explicit operator UserResponse(User u) => new UserResponse
    {
        UserId = u.Id,
        Name = u.Name,
        Email = u.Email.Address,
        Active = u.Active,
        Type = u.TypeUser,
        GameIds = u.LibraryGames?.Select(lg => lg.GameId).ToList() ?? []
    };
}  