using Fiap.CRM.Domain.OutboxAggregate;
using Fiap.CRM.Domain.UserAggregate;
using Fiap.CRM.Domain.UserAggregate.Entities;
using Fiap.CRM.Infra.Data.MapEntities;
using Microsoft.EntityFrameworkCore;

namespace Fiap.CRM.Infra.Data;

public class Context : DbContext 
{
    public Context(DbContextOptions<Context> options) : base(options)
    {
        Database.Migrate();
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<LibraryGame> LibraryGames { get; set; } = null!;
    public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UsersMap());
        modelBuilder.ApplyConfiguration(new LibraryGamesMap());
        modelBuilder.ApplyConfiguration(new OutboxMessageMap());
    }
}