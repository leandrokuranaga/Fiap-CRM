using Fiap.CRM.Domain.Common.ValueObjects;
using Fiap.CRM.Domain.UserAggregate;
using Fiap.CRM.Domain.UserAggregate.Entities;
using Fiap.CRM.Domain.UserAggregate.Enums;

namespace Fiap.CRM.Unit.Tests._3._Domain_Layer_Tests.Entities;

public class LibraryGamesDomainTests
{
    [Fact]
    public void ParameterlessConstructor_ShouldCreateLibraryGame()
    {
        // Act
        var libraryGame = new LibraryGame();

        // Assert
        Assert.NotNull(libraryGame);
        Assert.Equal(0, libraryGame.Id);
        Assert.Equal(0, libraryGame.GameId);
        Assert.Equal(0, libraryGame.UserId);
        Assert.Equal(DateTime.MinValue, libraryGame.PurchaseDate);
        Assert.Null(libraryGame.PricePaid);
        Assert.Null(libraryGame.User);
    }

    [Fact]
    public void Constructor_ShouldCreateLibraryGame_WhenValidParameters()
    {
        // Arrange
        var id = 1;
        var userId = 2;
        var gameId = 3;
        var purchaseDate = DateTime.UtcNow;
        var pricePaid = 59.99M;

        // Act
        var libraryGame = new LibraryGame(id, userId, gameId, purchaseDate, pricePaid);

        // Assert
        Assert.Equal(id, libraryGame.Id);
        Assert.Equal(userId, libraryGame.UserId);
        Assert.Equal(gameId, libraryGame.GameId);
        Assert.Equal(purchaseDate, libraryGame.PurchaseDate);
        Assert.NotNull(libraryGame.PricePaid);
        Assert.Equal(pricePaid, libraryGame.PricePaid.Value);
        Assert.Equal("BRL", libraryGame.PricePaid.Currency);
    }

    [Fact]
    public void Create_ShouldCreateLibraryGameWithZeroId()
    {
        // Arrange
        var userId = 10;
        var gameId = 15;
        var purchaseDate = DateTime.UtcNow;
        var pricePaid = 45.50M;

        // Act
        var libraryGame = LibraryGame.Create(userId, gameId, purchaseDate, pricePaid);

        // Assert
        Assert.Equal(0, libraryGame.Id);
        Assert.Equal(userId, libraryGame.UserId);
        Assert.Equal(gameId, libraryGame.GameId);
        Assert.Equal(purchaseDate, libraryGame.PurchaseDate);
        Assert.NotNull(libraryGame.PricePaid);
        Assert.Equal(pricePaid, libraryGame.PricePaid.Value);
        Assert.Equal("BRL", libraryGame.PricePaid.Currency);
    }

    [Fact]
    public void Properties_ShouldBeSettable()
    {
        // Arrange
        var libraryGame = new LibraryGame();
        var gameId = 100;
        var userId = 200;
        var purchaseDate = DateTime.UtcNow;
        var pricePaid = new Money(75.25M, "USD");

        // Act
        libraryGame.GameId = gameId;
        libraryGame.UserId = userId;
        libraryGame.PurchaseDate = purchaseDate;
        libraryGame.PricePaid = pricePaid;

        // Assert
        Assert.Equal(gameId, libraryGame.GameId);
        Assert.Equal(userId, libraryGame.UserId);
        Assert.Equal(purchaseDate, libraryGame.PurchaseDate);
        Assert.Equal(pricePaid, libraryGame.PricePaid);
    }

    [Fact]
    public void User_PropertyShouldBeSettable()
    {
        // Arrange
        var libraryGame = new LibraryGame();
        var user = new User("Test User", "test@example.com", "Password123!", TypeUser.User, true) { Id = 1 };

        // Act
        libraryGame.User = user;

        // Assert
        Assert.Equal(user, libraryGame.User);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10.50)]
    [InlineData(99.99)]
    [InlineData(1)]
    [InlineData(1000)]
    public void Constructor_ShouldAcceptValidPrices(decimal price)
    {
        // Arrange
        var purchaseDate = DateTime.UtcNow;

        // Act & Assert - Should not throw
        var libraryGame = new LibraryGame(1, 1, 1, purchaseDate, price);
        Assert.Equal(price, libraryGame.PricePaid.Value);
    }

    [Fact]
    public void LibraryGame_ShouldMaintainUserRelationship()
    {
        // Arrange
        var user = new User("Game User", "gamer@example.com", "Password123!", TypeUser.User, true) { Id = 5 };
        var libraryGame = LibraryGame.Create(user.Id, 25, DateTime.UtcNow, 39.99M);

        // Act
        libraryGame.User = user;

        // Assert
        Assert.Equal(user.Id, libraryGame.UserId);
        Assert.Equal(user, libraryGame.User);
    }

    [Fact]
    public void PricePaid_ShouldUseBRLAsDefaultCurrency()
    {
        // Arrange & Act
        var libraryGame = new LibraryGame(1, 1, 1, DateTime.UtcNow, 50.00M);

        // Assert
        Assert.Equal("BRL", libraryGame.PricePaid.Currency);
    }

    [Fact]
    public void LibraryGame_ShouldAllowDifferentGameIds()
    {
        // Arrange
        var userId = 1;
        var purchaseDate = DateTime.UtcNow;
        var price = 29.99M;

        // Act
        var game1 = LibraryGame.Create(userId, 100, purchaseDate, price);
        var game2 = LibraryGame.Create(userId, 200, purchaseDate, price);

        // Assert
        Assert.Equal(100, game1.GameId);
        Assert.Equal(200, game2.GameId);
        Assert.NotEqual(game1.GameId, game2.GameId);
    }

    [Fact]
    public void LibraryGame_ShouldAllowDifferentPurchaseDates()
    {
        // Arrange
        var userId = 1;
        var gameId = 1;
        var price = 29.99M;
        var date1 = DateTime.UtcNow;
        var date2 = DateTime.UtcNow.AddDays(-30);

        // Act
        var game1 = LibraryGame.Create(userId, gameId, date1, price);
        var game2 = LibraryGame.Create(userId, gameId, date2, price);

        // Assert
        Assert.Equal(date1, game1.PurchaseDate);
        Assert.Equal(date2, game2.PurchaseDate);
        Assert.NotEqual(game1.PurchaseDate, game2.PurchaseDate);
    }

    [Fact]
    public void LibraryGame_ShouldAllowDifferentPrices()
    {
        // Arrange
        var userId = 1;
        var gameId = 1;
        var purchaseDate = DateTime.UtcNow;

        // Act
        var game1 = LibraryGame.Create(userId, gameId, purchaseDate, 19.99M);
        var game2 = LibraryGame.Create(userId, gameId, purchaseDate, 49.99M);

        // Assert
        Assert.Equal(19.99M, game1.PricePaid.Value);
        Assert.Equal(49.99M, game2.PricePaid.Value);
        Assert.NotEqual(game1.PricePaid.Value, game2.PricePaid.Value);
    }

    [Fact]
    public void LibraryGame_ShouldInheritFromEntity()
    {
        // Arrange & Act
        var libraryGame = new LibraryGame();

        // Assert
        Assert.IsAssignableFrom<Abp.Domain.Entities.Entity>(libraryGame);
        Assert.True(libraryGame.GetType().BaseType?.Name == "Entity");
    }
}