using Fiap.Contracts.Events;
using Fiap.CRM.Domain.SeedWork;
using Fiap.CRM.Domain.UserAggregate;
using Fiap.CRM.Domain.UserAggregate.Events;
using Rebus.Handlers;

namespace Fiap.CRM.Infra.HandleBus.Handlers
{
    public class AddGameToUserLibraryHandler(IUserRepository userRepository, IEventStoreRepository storeRepository) : IHandleMessages<AddGameToUserLibraryIntegrationEvent>
    {
        public async Task Handle(AddGameToUserLibraryIntegrationEvent message)
        {
            var user = await userRepository.GetWithLibraryAsync(message.UserId);

            if (!user.LibraryGames.Any(x => x.GameId == message.GameId))
                user.AddGameToLibrary(message.GameId, message.PurchaseDate, message.PricePaidValue);

            await userRepository.UpdateAsync(user);
            await userRepository.SaveChangesAsync();

            var libraryGame = user.LibraryGames.FirstOrDefault(x => x.GameId == message.GameId);
            if (libraryGame is not null)
            {
                var domainEvent = new AddGameToLibraryEvent(libraryGame);
                await storeRepository.SaveAsync(domainEvent);
            }
        }
    }
}