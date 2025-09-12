using Fiap.CRM.Domain.Common.ValueObjects;
using Fiap.CRM.Domain.SeedWork;
using Fiap.CRM.Domain.SeedWork.Exceptions;
using Fiap.CRM.Domain.UserAggregate;
using Fiap.CRM.Domain.UserAggregate.Entities;
using Fiap.CRM.Domain.UserAggregate.Enums;
using Fiap.CRM.Domain.UserAggregate.ValueObjects;

namespace Fiap.CRM.Unit.Tests._3._Domain_Layer_Tests.Entities
{
    public class UserDomainTests
    {
        [Fact]
        public void Constructor_ShouldCreateUser_WhenValidParameters()
        {
            // Arrange
            var name = "Test User";
            var email = "test@example.com";
            var password = "Password123!";
            var typeUser = TypeUser.User;
            var active = true;

            // Act
            var user = new User(name, email, password, typeUser, active);

            // Assert
            Assert.Equal(name, user.Name);
            Assert.Equal(email, user.Email.Address);
            Assert.Equal(typeUser, user.TypeUser);
            Assert.Equal(active, user.Active);
            Assert.NotNull(user.Password);
        }

        [Fact]
        public void ParameterlessConstructor_ShouldCreateUser()
        {
            // Act
            var user = new User();

            // Assert
            Assert.NotNull(user);
            Assert.Null(user.Name);
            Assert.Null(user.Email);
            Assert.Null(user.Password);
            Assert.False(user.Active);
            Assert.Equal((TypeUser)0, user.TypeUser); // Default enum value
        }

        [Fact]
        public void Constructor_ShouldThrowException_WhenEmailIsInvalid()
        {
            // Act & Assert
            var ex = Assert.Throws<BusinessRulesException>(() =>
                new User("Test User", "", "Password123!", TypeUser.User, true));

            Assert.Equal("Email address cannot be empty.", ex.Message);
        }

        [Fact]
        public void Constructor_ShouldThrowException_WhenPasswordIsInvalid()
        {
            // Act & Assert
            var ex = Assert.Throws<BusinessRulesException>(() =>
                new User("Test User", "test@example.com", "weak", TypeUser.User, true));

            Assert.Equal("Password must be between 8 and 100 characters.", ex.Message);
        }

        [Fact]
        public void Constructor_ShouldCreateAdminUser_WhenTypeIsAdmin()
        {
            // Arrange & Act
            var user = new User("Admin User", "admin@example.com", "AdminPass123!", TypeUser.Admin, true);

            // Assert
            Assert.Equal(TypeUser.Admin, user.TypeUser);
        }

        [Fact]
        public void Constructor_ShouldCreateInactiveUser_WhenActiveIsFalse()
        {
            // Arrange & Act
            var user = new User("Inactive User", "inactive@example.com", "Pass123!", TypeUser.User, false);

            // Assert
            Assert.False(user.Active);
        }

        [Fact]
        public void Name_PropertySetter_ShouldSetName()
        {
            // Arrange
            var user = new User();

            // Act
            user.Name = "New Name";

            // Assert
            Assert.Equal("New Name", user.Name);
        }

        [Fact]
        public void Email_PropertySetter_ShouldSetEmail()
        {
            // Arrange
            var user = new User();
            var email = new Email("newemail@example.com");

            // Act
            user.Email = email;

            // Assert
            Assert.Equal("newemail@example.com", user.Email.Address);
        }

        [Fact]
        public void Password_PropertySetter_ShouldSetPassword()
        {
            // Arrange
            var user = new User();
            var password = new Password("NewPassword123!");

            // Act
            user.Password = password;

            // Assert
            Assert.NotNull(user.Password);
            Assert.Equal(password, user.Password);
        }

        [Fact]
        public void TypeUser_PropertySetter_ShouldSetTypeUser()
        {
            // Arrange
            var user = new User();

            // Act
            user.TypeUser = TypeUser.Admin;

            // Assert
            Assert.Equal(TypeUser.Admin, user.TypeUser);
        }

        [Fact]
        public void Active_PropertySetter_ShouldSetActive()
        {
            // Arrange
            var user = new User();

            // Act
            user.Active = true;

            // Assert
            Assert.True(user.Active);
        }

        [Fact]
        public void LibraryGames_Property_ShouldBeInitializedAsEmptyList()
        {
            // Arrange & Act
            var user = new User();

            // Assert
            Assert.NotNull(user.LibraryGames);
            Assert.Empty(user.LibraryGames);
        }

        [Fact]
        public void LibraryGames_Property_ShouldAllowAddingGames()
        {
            // Arrange
            var user = new User("Test User", "test@example.com", "Pass123!", TypeUser.User, true);
            var libraryGame = new LibraryGame
            {
                GameId = 1,
                PricePaid = new Money(50M, "BRL"),
                PurchaseDate = DateTime.UtcNow
            };

            // Act
            user.LibraryGames.Add(libraryGame);

            // Assert
            Assert.Single(user.LibraryGames);
            Assert.Equal(1, user.LibraryGames.First().GameId);
        }

        [Fact]
        public void Constructor_ShouldInitializeLibraryGames()
        {
            // Arrange & Act
            var user = new User("Test User", "test@example.com", "Pass123!", TypeUser.User, true);

            // Assert
            Assert.NotNull(user.LibraryGames);
            Assert.Empty(user.LibraryGames);
        }

        [Theory]
        [InlineData("user@example.com")]
        [InlineData("test.email@domain.com")]
        [InlineData("complex+email@subdomain.example.org")]
        public void Constructor_ShouldAcceptValidEmails(string validEmail)
        {
            // Act & Assert - Should not throw
            var user = new User("Test User", validEmail, "Password123!", TypeUser.User, true);
            Assert.Equal(validEmail.ToLowerInvariant(), user.Email.Address);
        }

        [Fact]
        public void User_ShouldImplementIAggregateRoot()
        {
            // Arrange & Act
            var user = new User("Test User", "test@example.com", "Pass123!", TypeUser.User, true);

            // Assert
            Assert.IsAssignableFrom<IAggregateRoot>(user);
        }

        [Fact]
        public void User_ShouldHaveIdProperty()
        {
            // Arrange & Act
            var user = new User("Test User", "test@example.com", "Pass123!", TypeUser.User, true);

            // Assert
            Assert.Equal(0, user.Id);
        }

        [Fact]
        public void User_ShouldAllowSettingId()
        {
            // Arrange
            var user = new User("Test User", "test@example.com", "Pass123!", TypeUser.User, true)
            {
                // Act
                Id = 42
            };

            // Assert
            Assert.Equal(42, user.Id);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Constructor_ShouldSetActiveStatusCorrectly(bool activeStatus)
        {
            // Arrange & Act
            var user = new User("Test", "test@example.com", "Password123!", TypeUser.User, activeStatus);

            // Assert
            Assert.Equal(activeStatus, user.Active);
        }

        [Theory]
        [InlineData(TypeUser.User)]
        [InlineData(TypeUser.Admin)]
        public void Constructor_ShouldSetUserTypeCorrectly(TypeUser userType)
        {
            // Arrange & Act
            var user = new User("Test", "test@example.com", "Password123!", userType, true);

            // Assert
            Assert.Equal(userType, user.TypeUser);
        }

        [Fact]
        public void Constructor_ShouldCreatePasswordValueObject()
        {
            // Arrange & Act
            var user = new User("Test", "test@example.com", "MyPassword123!", TypeUser.User, true);

            // Assert
            Assert.NotNull(user.Password);
            Assert.IsType<Password>(user.Password);
        }

        [Fact]
        public void Constructor_ShouldCreateEmailValueObject()
        {
            // Arrange & Act
            var user = new User("Test", "test@example.com", "MyPassword123!", TypeUser.User, true);

            // Assert
            Assert.NotNull(user.Email);
            Assert.IsType<Email>(user.Email);
        }

        [Fact]
        public void CreateByPublic_ShouldCreateUserWithDefaultValues()
        {
            // Arrange & Act
            var user = User.CreateByPublic("Public User", "public@example.com", "Password123!");

            // Assert
            Assert.Equal("Public User", user.Name);
            Assert.Equal("public@example.com", user.Email.Address);
            Assert.Equal(TypeUser.User, user.TypeUser);
            Assert.True(user.Active);
        }

        [Fact]
        public void CreateByAdmin_ShouldCreateUserWithSpecifiedValues()
        {
            // Arrange & Act
            var user = User.CreateByAdmin("Admin User", "admin@example.com", "Password123!", TypeUser.Admin, false);

            // Assert
            Assert.Equal("Admin User", user.Name);
            Assert.Equal("admin@example.com", user.Email.Address);
            Assert.Equal(TypeUser.Admin, user.TypeUser);
            Assert.False(user.Active);
        }

        [Fact]
        public void HasGame_ShouldReturnTrue_WhenUserHasGame()
        {
            // Arrange
            var user = new User("Test User", "test@example.com", "Pass123!", TypeUser.User, true);
            user.LibraryGames.Add(new LibraryGame { GameId = 1 });

            // Act & Assert
            Assert.True(user.HasGame(1));
        }

        [Fact]
        public void HasGame_ShouldReturnFalse_WhenUserDoesNotHaveGame()
        {
            // Arrange
            var user = new User("Test User", "test@example.com", "Pass123!", TypeUser.User, true);

            // Act & Assert
            Assert.False(user.HasGame(1));
        }

        [Fact]
        public void AddGameToLibrary_ShouldAddGame_WhenUserDoesNotHaveIt()
        {
            // Arrange
            var user = new User("Test User", "test@example.com", "Pass123!", TypeUser.User, true);
            user.Id = 1;

            // Act
            user.AddGameToLibrary(1, DateTime.UtcNow, 50M);

            // Assert
            Assert.Single(user.LibraryGames);
            Assert.Equal(1, user.LibraryGames.First().GameId);
        }

        [Fact]
        public void AddGameToLibrary_ShouldNotAddGame_WhenUserAlreadyHasIt()
        {
            // Arrange
            var user = new User("Test User", "test@example.com", "Pass123!", TypeUser.User, true);
            user.Id = 1;
            user.LibraryGames.Add(new LibraryGame { GameId = 1 });

            // Act
            user.AddGameToLibrary(1, DateTime.UtcNow, 50M);

            // Assert
            Assert.Single(user.LibraryGames);
        }
    }
}