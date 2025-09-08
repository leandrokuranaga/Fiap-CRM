using Fiap.CRM.Domain.UserAggregate;
using Fiap.CRM.Infra.Data.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Fiap.CRM.Infra.Data.Repositories;

public class UserRepository(Context context) : BaseRepository<User>(context), IUserRepository
{
    public async Task<User?> GetWithLibraryAsync(int userId, CancellationToken ct = default)
    {
        var user = await context.Users.Include(u => u.LibraryGames).Where(u => u.Id == userId).FirstOrDefaultAsync(ct);
        if (user != null)
        {
            System.Console.WriteLine($"[DEBUG] GetWithLibraryAsync User {user.Id} has {user.LibraryGames.Count} library games in repository");
        }
        return user;
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
        var users = await context.Users.Include(u => u.LibraryGames).ToListAsync(ct);
        System.Console.WriteLine($"[DEBUG] GetAllWithLibraryAsync returned {users.Count} users");
        foreach (var user in users)
        {
            System.Console.WriteLine($"[DEBUG] User {user.Id} has {user.LibraryGames.Count} library games in repository");
        }
        return users;
    }
}