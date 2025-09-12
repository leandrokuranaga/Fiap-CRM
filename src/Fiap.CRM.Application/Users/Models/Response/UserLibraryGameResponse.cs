using Fiap.CRM.Domain.UserAggregate.Entities;
using System.Diagnostics.CodeAnalysis;

namespace Fiap.CRM.Application.Users.Models.Response
{
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
                GameId = libraryGame.GameId,
                Name = "Unknown", // Será preenchido quando os dados do jogo estiverem disponíveis
                Genre = "Unknown", // Será preenchido quando os dados do jogo estiverem disponíveis
                OriginalPrice = 0, // Será preenchido quando os dados do jogo estiverem disponíveis
                OriginalCurrency = "BRL", // Padrão
                PricePaid = libraryGame.PricePaid.Value,
                PurchaseCurrency = libraryGame.PricePaid.Currency,
                PurchaseDate = libraryGame.PurchaseDate
            };
        }
    }
}
