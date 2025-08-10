using Fiap.CRM.Domain.Common.ValueObjects;
using Fiap.CRM.Domain.UserAggregate.Entities;
using Fiap.CRM.Infra.Data.MapEntities.Seeds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fiap.CRM.Infra.Data.MapEntities;

public class LibraryGamesMap : IEntityTypeConfiguration<LibraryGame>
{
    public void Configure(EntityTypeBuilder<LibraryGame> builder)
    {
        builder.ToTable("LibraryGames");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.GameId)
            .HasDatabaseName("IX_LibraryGames_GameId");

        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("IX_LibraryGames_UserId");

        builder.Property(x => x.PurchaseDate)
            .HasConversion(
                v => v.Value,
                v => new UtcDate(v) 
            )
            .IsRequired();

        builder.OwnsOne(x => x.PricePaid, price =>
        {
            price.Property(p => p.Value)
                .HasColumnName("PricePaid")
                .IsRequired();

            price.Property(p => p.Currency)
                .HasColumnName("PriceCurrency")
                .IsRequired()
                .HasMaxLength(3);

            price.HasData(
                new { LibraryGameId = 1, Value = 10.00M, Currency = "USD" },
                new { LibraryGameId = 2, Value = 15.00M, Currency = "USD" },
                new { LibraryGameId = 3, Value = 20.00M, Currency = "USD" },
                new { LibraryGameId = 4, Value = 28.99M, Currency = "USD" }
            );
        });

        builder.HasOne(x => x.User)
            .WithMany(x => x.LibraryGames)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasData(LibraryGameSeed.LibraryGames());
    }
}