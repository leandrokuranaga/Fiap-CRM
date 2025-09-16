namespace Fiap.CRM.Domain.SeedWork;

public interface IEventSourcing
{
    Guid Id { get; }
    DateTime OccurredOn { get; }
}