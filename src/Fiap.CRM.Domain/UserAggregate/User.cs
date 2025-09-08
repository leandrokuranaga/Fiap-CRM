using System.Text.Json.Serialization;
using Abp.Domain.Entities;
using Fiap.CRM.Domain.UserAggregate.Entities;
using Fiap.CRM.Domain.UserAggregate.Enums;
using Fiap.CRM.Domain.UserAggregate.ValueObjects;

namespace Fiap.CRM.Domain.UserAggregate;

public class User : Entity, SeedWork.IAggregateRoot
{
    public User()
    {
            
    }

    public User(string name, string email, string password, TypeUser typeUser, bool active)
    {
        Name = name;
        Email = new Email(email);
        Password = new Password(password);
        TypeUser = typeUser;
        Active = active;
    }

    public string Name { get; set; }
    public Email Email { get; set; }
    [JsonIgnore]
    public Password Password { get; set; }
    public TypeUser TypeUser { get; set; }
    public bool Active { get; set; }
    [JsonIgnore]
    public ICollection<LibraryGame> LibraryGames { get; set; } = [];

    public static User CreateByAdmin(string name, string email, string password, TypeUser typeUser, bool active)
    {
        return new User(name, email, password, typeUser, active);
    }

    public static User CreateByPublic(string name, string email, string password)
    {
        return new User(name, email, password, TypeUser.User, true);
    }

    public bool HasGame(int gameId) => LibraryGames.Any(x => x.GameId == gameId);

    public void AddGameToLibrary(int gameId, DateTime purchaseDate, decimal pricePaid)
    {
        if (HasGame(gameId)) return;

        var lg = LibraryGame.Create(userId: this.Id, gameId, purchaseDate, pricePaid);
        LibraryGames.Add(lg);
    }

}