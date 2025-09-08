namespace Fiap.Contracts.Events;

public sealed record AddGameToUserLibraryIntegrationEvent(
    int GameId,
    int UserId,
    DateTime PurchaseDate,
    decimal PricePaidValue,
    string? PricePaidCurrency = "BRL"
);
