using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fiap.CRM.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Type = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Content = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OccuredOn = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ProcessedOn = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PasswordHash = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PasswordSalt = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TypeUser = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LibraryGames",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    GameId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    PurchaseDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    PricePaid = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    PriceCurrency = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibraryGames", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LibraryGames_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Active", "Name", "TypeUser", "Email", "PasswordHash", "PasswordSalt" },
                values: new object[,]
                {
                    { 1, true, "Admin", 1, "admin@domain.com", "10000.zC3asvLZv6BQ2X3zcYdnhw==.d4+u5b59o6giayRcfePNPyF4I6nekoRp9bAMUEy3iHc=", "WQ1YZ/OiCeVcusBQ94njtQ==" },
                    { 2, true, "User", 2, "user@domain.com", "10000.SFG18S5pQd5QgxPtSxwPaw==.MPaE5q4K+T8EROdyuCeTz8SmJv+8q+h+yeN0d9pGUOg=", "J3CYQ56LlZ/D7NXAZBirlA==" }
                });

            migrationBuilder.InsertData(
                table: "LibraryGames",
                columns: new[] { "Id", "PriceCurrency", "PricePaid", "GameId", "PurchaseDate", "UserId" },
                values: new object[,]
                {
                    { 1, "USD", 10.00m, 1, new DateTime(2025, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1 },
                    { 2, "USD", 15.00m, 2, new DateTime(2025, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 2 },
                    { 3, "USD", 20.00m, 3, new DateTime(2025, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1 },
                    { 4, "USD", 28.99m, 4, new DateTime(2025, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_LibraryGames_GameId",
                table: "LibraryGames",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryGames_UserId",
                table: "LibraryGames",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LibraryGames");

            migrationBuilder.DropTable(
                name: "OutboxMessages");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
