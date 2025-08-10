using Fiap.CRM.Domain.SeedWork;

namespace Fiap.CRM.Domain.OutboxAggregate;

public interface IOutboxRepository : IBaseRepository<OutboxMessage>, IUnitOfWork
{
}