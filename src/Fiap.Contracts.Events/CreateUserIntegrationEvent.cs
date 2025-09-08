namespace Fiap.Contracts.Events
{
    public record CreateUserIntegrationEvent(
        int UserId,
        string Name,
        string Email,
        int Type,
        bool Active
    );
}
