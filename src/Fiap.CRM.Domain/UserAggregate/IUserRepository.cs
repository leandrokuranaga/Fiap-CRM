using Fiap.CRM.Domain.SeedWork;

namespace Fiap.CRM.Domain.UserAggregate;

public interface IUserRepository : IBaseRepository<User>, IUnitOfWork
{
    Task<User> GetByIdGameUserAsync(int id, bool noTracking);
}