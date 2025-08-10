using Fiap.CRM.Domain.SeedWork;

namespace Fiap.CRM.Domain.UserAggregate;

public interface IUserRepository : IBaseRepository<User>, IUnitOfWork
{
}