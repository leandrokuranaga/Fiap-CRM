using Fiap.CRM.Domain.SeedWork;

namespace Fiap.CRM.Domain.UserAggregate;

public interface IUserRepository : IBaseRepository<User>, IUnitOfWork
{
    Task<User?> GetWithLibraryAsync(int userId, CancellationToken ct = default);
    Task<bool> HasGameAsync(int userId, int gameId, CancellationToken ct = default);
    Task EnsureLibraryLoadedAsync(User user, CancellationToken ct = default);
}