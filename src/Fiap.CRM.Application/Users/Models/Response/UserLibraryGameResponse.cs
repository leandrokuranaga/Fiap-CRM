using System.Diagnostics.CodeAnalysis;
using Fiap.CRM.Domain.UserAggregate.Entities;

namespace Fiap.CRM.Application.Users.Models.Response;

[ExcludeFromCodeCoverage]
public record UserLibraryGameResponse
{
    public int GameId { get; set; }
    public string Name { get; set; } = null!;
    public string Genre { get; set; } = null!;
    public decimal OriginalPrice { get; set; }
    public string OriginalCurrency { get; set; } = null!;
    public decimal PricePaid { get; set; }
    public string PurchaseCurrency { get; set; } = null!;
    public DateTime PurchaseDate { get; set; }

    public static explicit operator UserLibraryGameResponse(LibraryGame libraryGame)
    {
        return new UserLibraryGameResponse
        {
            GameId = libraryGame.Game.Id,
            Name = libraryGame.Game.Name,
            Genre = libraryGame.Game.Genre,
            OriginalPrice = libraryGame.Game.Price.Value,
            OriginalCurrency = libraryGame.Game.Price.Currency,
            PricePaid = libraryGame.PricePaid.Value,
            PurchaseCurrency = libraryGame.PricePaid.Currency,
            PurchaseDate = libraryGame.PurchaseDate
        };
    }
}