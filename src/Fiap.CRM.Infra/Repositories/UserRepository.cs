using Fiap.CRM.Domain.UserAggregate;
using Fiap.CRM.Infra.Data.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Fiap.CRM.Infra.Data.Repositories;

public class UserRepository(Context context) : BaseRepository<User>(context), IUserRepository
{
    public async Task<User?> GetWithLibraryAsync(int userId, CancellationToken ct = default)
    {
        return await context.Users
            .AsNoTracking()
            .Include(u => u.LibraryGames)
            .Where(u => u.Id == userId)
            .FirstOrDefaultAsync(ct);
    }

    public Task<bool> HasGameAsync(int userId, int gameId, CancellationToken ct = default) =>
        dbSet.AsNoTracking()
             .Where(u => u.Id == userId)
             .SelectMany(u => u.LibraryGames)
             .AnyAsync(g => g.GameId == gameId, ct);

    public async Task EnsureLibraryLoadedAsync(User user, CancellationToken ct = default)
    {
        var coll = context.Entry(user).Collection(u => u.LibraryGames);
        if (!coll.IsLoaded)
            await coll.LoadAsync(ct);
    }

    public async Task<IEnumerable<User>> GetAllWithLibraryAsync(CancellationToken ct = default)
    {
        return await context.Users
            .AsNoTracking()
            .Include(u => u.LibraryGames)
            .ToListAsync(ct);
    }
}