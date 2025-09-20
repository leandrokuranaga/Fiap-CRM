using Fiap.Contracts.Events;
using Fiap.CRM.Domain.SeedWork;
using Fiap.CRM.Domain.UserAggregate.Events;
using Fiap.CRM.Infra.CrossCutting.Keycloak;
using Rebus.Handlers;

namespace Fiap.CRM.Infra.Bus.Handlers;

public class CreatedUserHandler(IEventStoreRepository eventStoreRepository, IKeycloakService keycloakService) : IHandleMessages<CreateUserIntegrationEvent>
{
    public async Task Handle(CreateUserIntegrationEvent message)
    {
        var userCreated = new CreatedUserEvent
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

        await eventStoreRepository.SaveAsync(userCreated);


        (string name, string lastName) = SplitName(message.Name);
        await keycloakService.CreateUserAsync(message.Email, name, lastName, message.Password, message.Type.ToString());
    }

    private static (string FirstName, string LastName) SplitName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return (string.Empty, string.Empty);
        }

        string[] parts = fullName.Trim().Split(' ');

        string firstName = parts[0];

        string lastName = string.Empty;
        if (parts.Length > 1)
        {
            lastName = string.Join(" ", parts, 1, parts.Length - 1);
        }

        return (firstName, lastName);
    }
}
