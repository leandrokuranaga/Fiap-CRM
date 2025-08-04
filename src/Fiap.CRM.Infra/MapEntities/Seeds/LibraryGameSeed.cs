using Fiap.CRM.Domain.Common.ValueObjects;
using Fiap.CRM.Domain.UserAggregate.Entities;

namespace Fiap.CRM.Infra.Data.MapEntities.Seeds;

public static class LibraryGameSeed
{
    public static List<LibraryGame> LibraryGames()
    {
        return
        [
            new LibraryGame
            {
                Id = 1,
                UserId = 1,
                GameId = 1,
                PurchaseDate = new UtcDate(DateTime.SpecifyKind(new DateTime(2025, 04, 01), DateTimeKind.Utc)),
            },
            new LibraryGame
            {
                Id = 2,
                UserId = 2,
                GameId = 2,
                PurchaseDate = new UtcDate(DateTime.SpecifyKind(new DateTime(2025, 05, 01), DateTimeKind.Utc)),
            },
            new LibraryGame
            {
                Id = 3,
                UserId = 1,
                GameId = 3,
                PurchaseDate = new UtcDate(DateTime.SpecifyKind(new DateTime(2025, 06, 01), DateTimeKind.Utc)),
            },
            new LibraryGame
            {
                Id = 4,
                UserId = 2,
                GameId = 4,
                PurchaseDate = new UtcDate(DateTime.SpecifyKind(new DateTime(2025, 07, 01), DateTimeKind.Utc)),
            }
        ];
    }
}