using Fiap.CRM.Domain.UserAggregate;
using Fiap.CRM.Infra.Data.MapEntities.Seeds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fiap.CRM.Infra.Data.MapEntities;

public class UsersMap : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.TypeUser)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.Active)
                .IsRequired()
                .HasDefaultValue(true);

            builder.HasMany(x => x.LibraryGames)
                   .WithOne(lg => lg.User)
                   .HasForeignKey(lg => lg.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.OwnsOne(x => x.Password, pw =>
            {
                pw.Property(p => p.Hash)
                  .HasColumnName("PasswordHash")
                  .IsRequired();

                pw.Property(p => p.PasswordSalt)
                  .HasColumnName("PasswordSalt")
                  .IsRequired();

                pw.WithOwner();

                pw.HasData(
                    new
                    {
                        UserId = 1,
                        Hash = "10000.zC3asvLZv6BQ2X3zcYdnhw==.d4+u5b59o6giayRcfePNPyF4I6nekoRp9bAMUEy3iHc=",
                        PasswordSalt = "WQ1YZ/OiCeVcusBQ94njtQ=="
                    },
                    new
                    {
                        UserId = 2,
                        Hash = "10000.SFG18S5pQd5QgxPtSxwPaw==.MPaE5q4K+T8EROdyuCeTz8SmJv+8q+h+yeN0d9pGUOg=",
                        PasswordSalt = "J3CYQ56LlZ/D7NXAZBirlA=="
                    });
            });

            builder.OwnsOne(u => u.Email, email =>
            {
                email.Property(e => e.Address)
                     .HasColumnName("Email")
                     .IsRequired()
                     .HasMaxLength(100);

                email.HasIndex(e => e.Address)
                     .IsUnique()
                     .HasDatabaseName("IX_Users_Email");

                email.HasData(
                    new { UserId = 1, Address = "admin@domain.com" },
                    new { UserId = 2, Address = "user@domain.com" }
                );
            });

            builder.HasData(UserSeed.Users());

        }
    }