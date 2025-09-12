using Fiap.CRM.Application.Users.Models.Request;
using Fiap.CRM.Application.Users.Models.Response;
using Fiap.CRM.Application.Users.Services;
using Fiap.CRM.Domain.Common.ValueObjects;
using Fiap.CRM.Domain.OutboxAggregate;
using Fiap.CRM.Domain.SeedWork;
using Fiap.CRM.Domain.SeedWork.Enums;
using Fiap.CRM.Domain.UserAggregate;
using Fiap.CRM.Domain.UserAggregate.Enums;
using Fiap.CRM.Infra.CacheService.Services;
using Fiap.CRM.Infra.CrossCutting.Http.FCG;
using Fiap.CRM.Infra.CrossCutting.Http.FCG.Models;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;

namespace Fiap.CRM.Unit.Tests._2._Application_Layer_Tests
{
    public class UsersServiceTests
    {
        private readonly Mock<INotification> _mockNotification;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ICacheService> _mockCache;
        private readonly Mock<IOutboxRepository> _mockOutboxRepository;
        private readonly Mock<IFCGService> _mockFcgService;
        private readonly Mock<ILogger<UsersService>> _mockLogger;
        private readonly UsersService _usersService;

        public UsersServiceTests()
        {
            _mockNotification = new Mock<INotification>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockCache = new Mock<ICacheService>();
            _mockOutboxRepository = new Mock<IOutboxRepository>();
            _mockFcgService = new Mock<IFCGService>();
            _mockLogger = new Mock<ILogger<UsersService>>();

            _usersService = new UsersService(
                _mockNotification.Object,
                _mockUserRepository.Object,
                _mockUnitOfWork.Object,
                _mockCache.Object,
                _mockOutboxRepository.Object,
                _mockFcgService.Object,
                _mockLogger.Object
            );
        }

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_ShouldReturnUsers_WhenUsersExist()
        {
            // Arrange
            var users = new List<User>
            {
                new("User 1", "user1@test.com", "Pass123!!", TypeUser.User, true) { Id = 1 },
                new("User 2", "user2@test.com", "Pass123!!", TypeUser.Admin, true) { Id = 2 }
            };

            _mockUserRepository
                .Setup(x => x.GetAllWithLibraryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(users);

            _mockCache
                .Setup(x => x.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<List<UserResponse>>>>()))
                .Returns<string, Func<Task<List<UserResponse>>>>((key, fallback) => fallback());

            // Act
            var result = await _usersService.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("User 1", result.First().Name);
            Assert.Equal("User 2", result.Skip(1).First().Name);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoUsers()
        {
            // Arrange
            var emptyUsers = new List<User>();

            _mockUserRepository
                .Setup(x => x.GetAllWithLibraryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(emptyUsers);

            _mockCache
                .Setup(x => x.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<List<UserResponse>>>>()))
                .Returns<string, Func<Task<List<UserResponse>>>>((key, fallback) => fallback());

            // Act
            var result = await _usersService.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllAsync_ShouldRemoveCacheFirst()
        {
            // Arrange
            var users = new List<User>();

            _mockUserRepository
                .Setup(x => x.GetAllWithLibraryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(users);

            _mockCache
                .Setup(x => x.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<List<UserResponse>>>>()))
                .Returns<string, Func<Task<List<UserResponse>>>>((key, fallback) => fallback());

            // Act
            await _usersService.GetAllAsync();

            // Assert
            _mockCache.Verify(x => x.RemoveAsync(EnumCacheTags.AllUsers), Times.Once);
        }

        #endregion

        #region GetAsync Tests

        [Fact]
        public async Task GetAsync_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var userId = 1;
            var user = new User("Test User", "test@example.com", "Pass123!!", TypeUser.User, true) { Id = userId };
            var userResponse = (UserResponse)user;

            _mockCache
                .Setup(x => x.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<UserResponse>>>()))
                .ReturnsAsync(userResponse);

            // Act
            var result = await _usersService.GetAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.UserId);
            Assert.Equal("Test User", result.Name);
            Assert.Equal("test@example.com", result.Email);
        }

        [Fact]
        public async Task GetAsync_ShouldAddNotification_WhenUserNotFound()
        {
            // Arrange
            var userId = 999;

            _mockCache
                .Setup(x => x.GetOrSetAsync<UserResponse>(It.IsAny<string>(), It.IsAny<Func<Task<UserResponse>>>()))
                .ReturnsAsync((UserResponse?)null);

            // Act
            var result = await _usersService.GetAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.UserId);
            _mockNotification.Verify(x => x.AddNotification(
                "Get User",
                "User not found",
                It.IsAny<NotificationModel.ENotificationType>()), Times.Once);
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_ShouldReturnUserResponse_WhenValidRequest()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                Name = "New User",
                Email = "new@test.com",
                Password = "Password123@"
            };

            _mockUserRepository
                .Setup(x => x.ExistAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(false);

            _mockUserRepository
                .Setup(x => x.InsertOrUpdateAsync(It.IsAny<User>()))
                .ReturnsAsync((User u) =>
                {
                    u.Id = 1;
                    return u;
                });

            _mockUserRepository.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);
            _mockOutboxRepository.Setup(x => x.InsertOrUpdateAsync(It.IsAny<OutboxMessage>())).ReturnsAsync((OutboxMessage o) => o);
            _mockOutboxRepository.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);
            _mockCache.Setup(x => x.RemoveAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

            // Act
            var result = await _usersService.CreateAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.UserId);
            Assert.Equal("New User", result.Name);
            Assert.Equal("new@test.com", result.Email);
            Assert.True(result.Active);
            Assert.Equal(TypeUser.User, result.Type);
        }

        [Fact]
        public async Task CreateAsync_ShouldAddNotification_WhenEmailExists()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                Name = "Duplicate User",
                Email = "existing@test.com",
                Password = "Password123@"
            };

            _mockUserRepository
                .Setup(x => x.ExistAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(true);

            // Act
            var result = await _usersService.CreateAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.UserId);
            _mockNotification.Verify(x => x.AddNotification(
                "Create User",
                "Email already registered",
                NotificationModel.ENotificationType.BusinessRules), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateOutboxMessage()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                Name = "User for Outbox",
                Email = "outbox@test.com",
                Password = "Password123@"
            };

            _mockUserRepository
                .Setup(x => x.ExistAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(false);

            _mockUserRepository
                .Setup(x => x.InsertOrUpdateAsync(It.IsAny<User>()))
                .ReturnsAsync((User u) =>
                {
                    u.Id = 5;
                    return u;
                });

            _mockUserRepository.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);
            _mockOutboxRepository.Setup(x => x.InsertOrUpdateAsync(It.IsAny<OutboxMessage>())).ReturnsAsync((OutboxMessage o) => o);
            _mockOutboxRepository.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);
            _mockCache.Setup(x => x.RemoveAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

            // Act
            await _usersService.CreateAsync(request);

            // Assert
            _mockOutboxRepository.Verify(x => x.InsertOrUpdateAsync(It.IsAny<OutboxMessage>()), Times.Once);
        }

        #endregion

        #region CreateAdminAsync Tests

        [Fact]
        public async Task CreateAdminAsync_ShouldReturnAdminUser_WhenValidRequest()
        {
            // Arrange
            var request = new CreateUserAdminRequest
            {
                Name = "Admin User",
                Email = "admin@test.com",
                Password = "AdminPass123@!",
                TypeUser = TypeUser.Admin,
                Active = true
            };

            _mockUserRepository
                .Setup(x => x.ExistAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(false);

            _mockUserRepository
                .Setup(x => x.InsertOrUpdateAsync(It.IsAny<User>()))
                .ReturnsAsync((User u) =>
                {
                    u.Id = 2;
                    return u;
                });

            _mockUserRepository.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);
            _mockOutboxRepository.Setup(x => x.InsertOrUpdateAsync(It.IsAny<OutboxMessage>())).ReturnsAsync((OutboxMessage o) => o);
            _mockOutboxRepository.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);
            _mockCache.Setup(x => x.RemoveAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

            // Act
            var result = await _usersService.CreateAdminAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.UserId);
            Assert.Equal("Admin User", result.Name);
            Assert.Equal("admin@test.com", result.Email);
            Assert.True(result.Active);
            Assert.Equal(TypeUser.Admin, result.Type);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_ShouldReturnSuccess_WhenUserExists()
        {
            // Arrange
            var userId = 1;
            var request = new UpdateUserRequest
            {
                Name = "Updated Name",
                Email = "updated@test.com"
            };

            var existingUser = new User("Old Name", "old@test.com", "Pass123!", TypeUser.User, true) { Id = userId };

            _mockUserRepository
                .Setup(x => x.GetByIdAsync(userId, true))
                .ReturnsAsync(existingUser);

            _mockUserRepository.Setup(x => x.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
            _mockUserRepository.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);
            _mockCache.Setup(x => x.RemoveAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

            // Act
            var result = await _usersService.UpdateAsync(userId, request);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldAddNotification_WhenUserNotFound()
        {
            // Arrange
            var userId = 999;
            var request = new UpdateUserRequest { Name = "Non-existent" };

            _mockUserRepository
                .Setup(x => x.GetByIdAsync(userId, true))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _usersService.UpdateAsync(userId, request);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            _mockNotification.Verify(x => x.AddNotification(
                "Update User",
                "User not found",
                NotificationModel.ENotificationType.NotFound), Times.Once);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_ShouldReturnSuccess_WhenUserExists()
        {
            // Arrange
            var userId = 1;
            var user = new User("User to Delete", "delete@test.com", "Pass123!", TypeUser.User, true) { Id = userId };

            _mockUserRepository
                .Setup(x => x.GetByIdAsync(userId, true))
                .ReturnsAsync(user);

            _mockUserRepository.Setup(x => x.DeleteAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
            _mockUserRepository.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);
            _mockCache.Setup(x => x.RemoveAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

            // Act
            var result = await _usersService.DeleteAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            _mockUserRepository.Verify(x => x.DeleteAsync(user), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldAddNotification_WhenUserNotFound()
        {
            // Arrange
            var userId = 999;

            _mockUserRepository
                .Setup(x => x.GetByIdAsync(userId, true))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _usersService.DeleteAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            _mockNotification.Verify(x => x.AddNotification(
                "Delete User",
                "User Not found",
                NotificationModel.ENotificationType.NotFound), Times.Once);
        }

        #endregion

        #region GetGamesByUserAsync Tests

        [Fact]
        public async Task GetGamesByUserAsync_ShouldReturnUserGames_WhenUserHasGames()
        {
            // Arrange
            var userId = 1;
            var user = new User("Gamer User", "gamer@test.com", "Pass123!", TypeUser.User, true)
            {
                Id = userId,
                LibraryGames = new List<Domain.UserAggregate.Entities.LibraryGame>
                {
                    new Domain.UserAggregate.Entities.LibraryGame
                    {
                        GameId = 1,
                        PricePaid = new Money(45M, "BRL"),
                        PurchaseDate = DateTime.UtcNow
                    }
                }
            };

            var gamesResponse = new ApiEnvelope<List<GameResponse>>(
                new List<GameResponse>
                {
                    new GameResponse
                    {
                        Id = 1,
                        Name = "Test Game",
                        Genre = "Action",
                        Price = 60M,
                        PriceWithDiscount = 45M
                    }
                },
                true,
                null
            ); 

            _mockUserRepository
                .Setup(x => x.GetWithLibraryAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _mockFcgService
                .Setup(x => x.GetAllGamesAsync())
                .ReturnsAsync(gamesResponse);

            _mockCache
                .Setup(x => x.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<List<UserLibraryGameResponse>>>>()))
                .Returns<string, Func<Task<List<UserLibraryGameResponse>>>>((key, fallback) => fallback());

            // Act
            var result = await _usersService.GetGamesByUserAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(1, result.First().GameId);
            Assert.Equal("Test Game", result.First().Name);
            Assert.Equal(45M, result.First().PricePaid);
        }

        [Fact]
        public async Task GetGamesByUserAsync_ShouldReturnEmpty_WhenUserNotFound()
        {
            // Arrange
            var userId = 999;

            _mockUserRepository
                .Setup(x => x.GetWithLibraryAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            _mockCache
                .Setup(x => x.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<List<UserLibraryGameResponse>>>>()))
                .Returns<string, Func<Task<List<UserLibraryGameResponse>>>>((key, fallback) => fallback());

            // Act
            var result = await _usersService.GetGamesByUserAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetGamesByUserAsync_ShouldReturnEmpty_WhenFCGServiceFails()
        {
            // Arrange
            var userId = 1;
            var user = new User("User", "user@test.com", "Pass123!", TypeUser.User, true)
            {
                Id = userId,
                LibraryGames = new List<Domain.UserAggregate.Entities.LibraryGame>()
            };

            var failedResponse = new ApiEnvelope<List<GameResponse>>(null, false, "Service unavailable");

            _mockUserRepository
                .Setup(x => x.GetWithLibraryAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _mockFcgService
                .Setup(x => x.GetAllGamesAsync())
                .ReturnsAsync(failedResponse);

            _mockCache
                .Setup(x => x.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<List<UserLibraryGameResponse>>>>()))
                .Returns<string, Func<Task<List<UserLibraryGameResponse>>>>((key, fallback) => fallback());

            // Act
            var result = await _usersService.GetGamesByUserAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetGamesByUserAsync_ShouldAddNotification_WhenUserHasNoGames()
        {
            // Arrange
            var userId = 1;

            _mockCache
                .Setup(x => x.GetOrSetAsync<List<UserLibraryGameResponse>>(It.IsAny<string>(), It.IsAny<Func<Task<List<UserLibraryGameResponse>>>>()))
                .ReturnsAsync(new List<UserLibraryGameResponse>());

            // Act
            var result = await _usersService.GetGamesByUserAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockNotification.Verify(x => x.AddNotification(
                "Get User",
                "User not found or has no games",
                NotificationModel.ENotificationType.NotFound), Times.Once);
        }

        #endregion

        #region Cache Tests

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(100)]
        public async Task GetGamesByUserAsync_ShouldUseCacheCorrectly(int userId)
        {
            // Arrange
            var cachedGames = new List<UserLibraryGameResponse>
            {
                new UserLibraryGameResponse { GameId = 1, Name = "Cached Game" }
            };

            _mockCache
                .Setup(x => x.GetOrSetAsync(EnumCacheTags.UserGames(userId), It.IsAny<Func<Task<List<UserLibraryGameResponse>>>>()))
                .ReturnsAsync(cachedGames);

            // Act
            var result = await _usersService.GetGamesByUserAsync(userId);

            // Assert
            Assert.Single(result);
            Assert.Equal("Cached Game", result.First().Name);
            _mockCache.Verify(x => x.GetOrSetAsync(EnumCacheTags.UserGames(userId), It.IsAny<Func<Task<List<UserLibraryGameResponse>>>>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ShouldInvalidateCache()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                Name = "Cache Test",
                Email = "cache@test.com",
                Password = "Password123@"
            };

            _mockUserRepository
                .Setup(x => x.ExistAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(false);

            _mockUserRepository
                .Setup(x => x.InsertOrUpdateAsync(It.IsAny<User>()))
                .ReturnsAsync((User u) => { u.Id = 1; return u; });

            SetupBasicMocks();

            // Act
            await _usersService.CreateAsync(request);

            // Assert
            _mockCache.Verify(x => x.RemoveAsync(EnumCacheTags.AllUsers), Times.Once);
        }

        #endregion

        private void SetupBasicMocks()
        {
            _mockUserRepository.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);
            _mockOutboxRepository.Setup(x => x.InsertOrUpdateAsync(It.IsAny<OutboxMessage>())).ReturnsAsync((OutboxMessage o) => o);
            _mockOutboxRepository.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);
            _mockCache.Setup(x => x.RemoveAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        }
    }
}
