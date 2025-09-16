using Fiap.CRM.Domain.SeedWork;
using Fiap.CRM.Domain.UserAggregate.Entities;
using System.Text.Json;

namespace Fiap.CRM.Domain.UserAggregate.Events;


public record AddGameToLibraryEvent : StoredEvent
{
    public int LibraryId { get; set; }
    public int GameId { get; set; }
    public int UserId { get; set; }

    public AddGameToLibraryEvent()
    {

    }

    public AddGameToLibraryEvent(LibraryGame library)
    {
        Id = Guid.NewGuid();
        LibraryId = library.Id;
        GameId = library.GameId;
        UserId = library.UserId;
        OccurredOn = DateTime.UtcNow;
        StreamName = $"{nameof(LibraryGame)}-{library.Id}";
        Type = nameof(LibraryGame);
        Data = JsonSerializer.Serialize(new
        {
            library.Id,
            library.UserId,
            library.GameId,
            library.PurchaseDate,
            library.PricePaid
        });
    }
}