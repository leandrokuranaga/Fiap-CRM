using Abp.Domain.Entities;
using Fiap.CRM.Domain.Common.ValueObjects;
using System.Text.Json.Serialization;

namespace Fiap.CRM.Domain.UserAggregate.Entities;

public class LibraryGame : Entity
{
    public LibraryGame()
    {            
    }

    public LibraryGame(int id, int userId, int gameId, DateTime purchaseDate, decimal pricePaid)
    {
        Id = id;
        UserId = userId;
        GameId = gameId;
        PurchaseDate = purchaseDate;
        PricePaid = new Money(pricePaid);
    }

    public int GameId { get; set; }
    public int UserId { get; set; }
    public DateTime PurchaseDate { get; set; }
    public Money PricePaid { get; set; }
    [JsonIgnore]
    public virtual User User { get; set; }

    public static LibraryGame Create(int userId, int gameId, DateTime purchaseDate, decimal pricePaid)
    {
        return new LibraryGame(0, userId, gameId, purchaseDate, pricePaid);
    }
}