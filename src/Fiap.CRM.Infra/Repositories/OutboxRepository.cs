using Fiap.CRM.Domain.OutboxAggregate;
using Fiap.CRM.Infra.Data.Repositories.Base;

namespace Fiap.CRM.Infra.Data.Repositories;

public class OutboxRepository(Context context) : BaseRepository<OutboxMessage>(context), IOutboxRepository
{
}