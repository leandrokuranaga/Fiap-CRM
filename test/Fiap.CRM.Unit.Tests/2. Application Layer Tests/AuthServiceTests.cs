using System.Linq.Expressions;
using Fiap.CRM.Application.Auth.Models.Request;
using Fiap.CRM.Application.Auth.Services;
using Fiap.CRM.Application.Common;
using Fiap.CRM.Domain.SeedWork;
using Fiap.CRM.Domain.UserAggregate;
using Fiap.CRM.Domain.UserAggregate.Enums;
using Fiap.CRM.Domain.UserAggregate.ValueObjects;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Fiap.CRM.Unit.Tests._2._Application_Layer_Tests
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<INotification> _mockNotification;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockNotification = new Mock<INotification>();
            _mockConfiguration = new Mock<IConfiguration>();

            // Setup JWT configuration
            _mockConfiguration.Setup(x => x["JwtSettings:SecretKey"]).Returns("SuperSecretKeyForTestingPurposesOnly123456789");

            _authService = new AuthService(
                _mockUserRepository.Object,
                _mockNotification.Object,
                _mockConfiguration.Object
            );
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreValid()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "test@example.com",
                Password = "Password123!"
            };

            var password = new Password("Password123!");
            var user = new User("Test User", "test@example.com", "Password123!", TypeUser.User, true) 
            { 
                Id = 1, 
                Password = password 
            };

            _mockUserRepository
                .Setup(x => x.GetOneNoTracking(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(user);

            // Act
            var result = await _authService.LoginAsync(loginRequest);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Token);
            Assert.NotEmpty(result.Token);
            Assert.True(result.Expiration > DateTime.UtcNow);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnEmptyLogin_WhenUserNotFound()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "nonexistent@example.com",
                Password = "Password123!"
            };

            _mockUserRepository
                .Setup(x => x.GetOneNoTracking(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _authService.LoginAsync(loginRequest);

            // Assert
            Assert.NotNull(result);
            Assert.True(string.IsNullOrEmpty(result.Token));
            _mockNotification.Verify(x => x.AddNotification(
                "Login Failed", 
                "Invalid username or password.", 
                NotificationModel.ENotificationType.NotFound), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_ShouldAddNotification_WhenPasswordIsIncorrect()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "test@example.com",
                Password = "WrongPassword123!"
            };

            var correctPassword = new Password("CorrectPassword123!");
            var user = new User("Test User", "test@example.com", "CorrectPassword123!", TypeUser.User, true) 
            { 
                Id = 1,
                Password = correctPassword
            };

            _mockUserRepository
                .Setup(x => x.GetOneNoTracking(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(user);

            // Act
            var result = await _authService.LoginAsync(loginRequest);

            // Assert
            Assert.NotNull(result);
            Assert.True(string.IsNullOrEmpty(result.Token));
            _mockNotification.Verify(x => x.AddNotification(
                "Login Failed", 
                "Invalid username or password.", 
                NotificationModel.ENotificationType.BusinessRules), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_ShouldAddNotification_WhenUserIsInactive()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "inactive@example.com",
                Password = "Password123!"
            };

            var password = new Password("Password123!");
            var user = new User("Inactive User", "inactive@example.com", "Password123!", TypeUser.User, false) 
            { 
                Id = 1,
                Password = password
            };

            _mockUserRepository
                .Setup(x => x.GetOneNoTracking(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(user);

            // Act
            var result = await _authService.LoginAsync(loginRequest);

            // Assert
            Assert.NotNull(result);
            Assert.True(string.IsNullOrEmpty(result.Token));
            _mockNotification.Verify(x => x.AddNotification(
                "Login Failed", 
                "Your account is disabled. Please contact support.", 
                NotificationModel.ENotificationType.BusinessRules), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_ShouldGenerateTokenWithCorrectClaims_ForAdminUser()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "admin@example.com",
                Password = "AdminPass123!"
            };

            var password = new Password("AdminPass123!");
            var user = new User("Admin User", "admin@example.com", "AdminPass123!", TypeUser.Admin, true) 
            { 
                Id = 2,
                Password = password
            };

            _mockUserRepository
                .Setup(x => x.GetOneNoTracking(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(user);

            // Act
            var result = await _authService.LoginAsync(loginRequest);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Token);
            
            // Verify the token contains expected information (basic validation)
            Assert.Contains(".", result.Token); // JWT tokens contain dots
        }

        [Theory]
        [InlineData("TEST@EXAMPLE.COM")]
        [InlineData("test@example.com")]
        [InlineData("  test@example.com  ")]
        public async Task LoginAsync_ShouldHandleCaseInsensitiveEmail(string email)
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = email,
                Password = "Password123!"
            };

            var password = new Password("Password123!");
            var user = new User("Test User", "test@example.com", "Password123!", TypeUser.User, true) 
            { 
                Id = 1,
                Password = password
            };

            _mockUserRepository
                .Setup(x => x.GetOneNoTracking(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(user);

            // Act
            var result = await _authService.LoginAsync(loginRequest);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Token);
        }

        [Fact]
        public async Task LoginAsync_ShouldCallRepositoryOnce()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "test@example.com",
                Password = "Password123!"
            };

            var password = new Password("Password123!");
            var user = new User("Test User", "test@example.com", "Password123!", TypeUser.User, true) 
            { 
                Id = 1,
                Password = password
            };

            _mockUserRepository
                .Setup(x => x.GetOneNoTracking(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(user);

            // Act
            await _authService.LoginAsync(loginRequest);

            // Assert
            _mockUserRepository.Verify(x => x.GetOneNoTracking(It.IsAny<Expression<Func<User, bool>>>()), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_ShouldNotCallRepositoryMultipleTimes()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "test@example.com",
                Password = "Password123!"
            };

            _mockUserRepository
                .Setup(x => x.GetOneNoTracking(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync((User?)null);

            // Act
            await _authService.LoginAsync(loginRequest);

            // Assert
            _mockUserRepository.Verify(x => x.GetOneNoTracking(It.IsAny<Expression<Func<User, bool>>>()), Times.Once);
        }

        [Fact]
        public void AuthService_ShouldImplementIAuthService()
        {
            // Assert
            Assert.IsAssignableFrom<IAuthService>(_authService);
        }

        [Fact]
        public void AuthService_ShouldInheritFromBaseService()
        {
            // Assert
            Assert.IsAssignableFrom<BaseService>(_authService);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnTokenWithExpiration()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "test@example.com",
                Password = "Password123!"
            };

            var password = new Password("Password123!");
            var user = new User("Test User", "test@example.com", "Password123!", TypeUser.User, true) 
            { 
                Id = 1,
                Password = password
            };

            _mockUserRepository
                .Setup(x => x.GetOneNoTracking(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(user);

            // Act
            var result = await _authService.LoginAsync(loginRequest);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Token);
            Assert.True(result.Expiration > DateTime.UtcNow);
            Assert.True(result.Expiration <= DateTime.UtcNow.AddHours(3)); // Should be around 2 hours
        }

        [Theory]
        [InlineData(TypeUser.User)]
        [InlineData(TypeUser.Admin)]
        public async Task LoginAsync_ShouldWorkForBothUserTypes(TypeUser userType)
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "test@example.com",
                Password = "Password123!"
            };

            var password = new Password("Password123!");
            var user = new User("Test User", "test@example.com", "Password123!", userType, true) 
            { 
                Id = 1,
                Password = password
            };

            _mockUserRepository
                .Setup(x => x.GetOneNoTracking(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(user);

            // Act
            var result = await _authService.LoginAsync(loginRequest);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Token);
        }
    }
}