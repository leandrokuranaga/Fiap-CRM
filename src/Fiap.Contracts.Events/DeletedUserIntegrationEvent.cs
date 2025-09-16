namespace Fiap.Contracts.Events;

public record DeletedUserIntegrationEvent(
    int UserId,
    string Name,
    string Email,
    int Type,
    bool Active
);