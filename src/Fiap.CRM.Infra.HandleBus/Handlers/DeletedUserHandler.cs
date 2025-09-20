using Fiap.Contracts.Events;
using Fiap.CRM.Domain.SeedWork;
using Fiap.CRM.Domain.UserAggregate.Events;
using Rebus.Handlers;

namespace Fiap.CRM.Infra.Bus.Handlers;

public class DeletedUserHandler(IEventStoreRepository eventStoreRepository) : IHandleMessages<DeletedUserIntegrationEvent>
{
    public async Task Handle(DeletedUserIntegrationEvent message)
    {
        var userDeleted = new DeletedUserEvent
        {
            Id = Guid.NewGuid(),
            UserId = message.UserId,
            Name = message.Name,
            OccurredOn = DateTime.UtcNow,
            StreamName = $"User-{message.UserId}",
            Type = "User",
            Data = System.Text.Json.JsonSerializer.Serialize(new {
                message.UserId,
                message.Name,
                message.Email,
                message.Type,
                message.Active
            })
        };
        await eventStoreRepository.SaveAsync(userDeleted);
    }
}
