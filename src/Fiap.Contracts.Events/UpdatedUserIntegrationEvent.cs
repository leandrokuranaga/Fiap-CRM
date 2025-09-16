namespace Fiap.Contracts.Events;

public record UpdatedUserIntegrationEvent(
    int UserId,
    string Name,
    string Email,
    int Type,
    bool Active
);