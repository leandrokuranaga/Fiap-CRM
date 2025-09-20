namespace Fiap.CRM.Domain.SeedWork;

public interface IEventStoreRepository
{
    Task SaveAsync<T>(T @event) where T : StoredEvent;
    Task<IEnumerable<StoredEvent>> GetEventsAsync(string aggregate, string streamName);
}