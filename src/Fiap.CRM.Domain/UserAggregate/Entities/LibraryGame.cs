using System.Text.Json.Serialization;
using Abp.Domain.Entities;
using Fiap.CRM.Domain.Common.ValueObjects;

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
        PurchaseDate = new UtcDate(purchaseDate);
        PricePaid = new Money(pricePaid);
    }

    public int GameId { get; set; }
    public int UserId { get; set; }
    public UtcDate PurchaseDate { get; set; }
    public Money PricePaid { get; set; }

    public virtual User User { get; set; }
}