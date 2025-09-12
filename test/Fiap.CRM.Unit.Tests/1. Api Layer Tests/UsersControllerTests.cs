using Fiap.CRM.Api.Controllers;
using Fiap.CRM.Application.Common;
using Fiap.CRM.Application.Users.Models.Request;
using Fiap.CRM.Application.Users.Models.Response;
using Fiap.CRM.Application.Users.Services;
using Fiap.CRM.Domain.SeedWork;
using Fiap.CRM.Domain.UserAggregate.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Moq;
using System.Security.Claims;

namespace Fiap.CRM.Unit.Tests._1._Api_Layer_Tests
{
    public class UsersControllerTests
    {
        private readonly Mock<IUsersService> _usersServiceMock;
        private readonly Mock<INotification> _notificationMock;
        private readonly UsersController _controller;

        public UsersControllerTests()
        {
            _usersServiceMock = new Mock<IUsersService>();
            _notificationMock = new Mock<INotification>();
            _controller = new UsersController(_usersServiceMock.Object, _notificationMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext(),
                    RouteData = new RouteData()
                }
            };
            _controller.ControllerContext.RouteData.Values["controller"] = "Users";
            _controller.ControllerContext.RouteData.Values["version"] = "1.0";
        }

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_ShouldReturnCreated_WhenValidRequest()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                Name = "Test User",
                Email = "test@example.com",
                Password = "Password123@"
            };

            var response = new UserResponse
            {
                UserId = 1,
                Name = "Test User",
                Email = "test@example.com",
                Active = true,
                Type = TypeUser.User
            };

            _usersServiceMock
                .Setup(x => x.CreateAsync(request))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.CreateAsync(request);

            // Assert
            var createdResult = Assert.IsType<CreatedResult>(result);
            var anonymousResponse = createdResult.Value;

            var successProp = anonymousResponse.GetType().GetProperty("success");
            var dataProp = anonymousResponse.GetType().GetProperty("data");

            Assert.True((bool)successProp.GetValue(anonymousResponse));
            var userData = (UserResponse)dataProp.GetValue(anonymousResponse);
            Assert.Equal(response.Name, userData.Name);
            Assert.Equal(response.Email, userData.Email);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnCreated_WhenServiceReturnsEmptyUserId()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                Name = "Invalid User",
                Email = "invalid@example.com",
                Password = "Password123@"
            };

            var response = new UserResponse { UserId = 0 };

            _usersServiceMock
                .Setup(x => x.CreateAsync(request))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.CreateAsync(request);

            // Assert
            var createdResult = Assert.IsType<CreatedResult>(result);
            Assert.Equal(201, createdResult.StatusCode);
            Assert.Equal("/api/v1.0/Users/0", createdResult.Location);
        }

        #endregion

        #region CreateAdminAsync Tests

        [Fact]
        public async Task CreateAdminAsync_ShouldReturnCreated_WhenValidRequest()
        {
            // Arrange
            var request = new CreateUserAdminRequest
            {
                Name = "Admin User",
                Email = "admin@example.com",
                Password = "AdminPass123@",
                TypeUser = TypeUser.Admin,
                Active = true
            };

            var response = new UserResponse
            {
                UserId = 2,
                Name = "Admin User",
                Email = "admin@example.com",
                Active = true,
                Type = TypeUser.Admin
            };

            _usersServiceMock
                .Setup(x => x.CreateAdminAsync(request))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.CreateAdminAsync(request);

            // Assert
            var createdResult = Assert.IsType<CreatedResult>(result);
            var anonymousResponse = createdResult.Value;

            var successProp = anonymousResponse.GetType().GetProperty("success");
            var dataProp = anonymousResponse.GetType().GetProperty("data");

            Assert.True((bool)successProp.GetValue(anonymousResponse));
            var userData = (UserResponse)dataProp.GetValue(anonymousResponse);
            Assert.Equal(response.Type, userData.Type);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_ShouldReturnNoContent_WhenValidRequest()
        {
            // Arrange
            var userId = 1;
            var request = new UpdateUserRequest
            {
                Name = "Updated Name",
                Email = "updated@example.com"
            };

            var response = BaseResponse<object>.Ok(null);

            _usersServiceMock
                .Setup(x => x.UpdateAsync(userId, request))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.UpdateAsync(userId, request);

            // Assert
            var noContentResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, noContentResult.StatusCode);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_ShouldReturnNoContent_WhenValidId()
        {
            // Arrange
            var userId = 1;
            var response = BaseResponse<object>.Ok(null);

            _usersServiceMock
                .Setup(x => x.DeleteAsync(userId))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.DeleteAsync(userId);

            // Assert
            var noContentResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, noContentResult.StatusCode);
        }

        #endregion

        #region GetAsync Tests

        [Fact]
        public async Task GetAsync_ShouldReturnOk_WhenUserExists()
        {
            // Arrange
            var userId = 1;
            var response = new UserResponse
            {
                UserId = userId,
                Name = "Test User",
                Email = "test@example.com",
                Active = true,
                Type = TypeUser.User 
            };

            _usersServiceMock
                .Setup(x => x.GetAsync(userId))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetAsync(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var baseResponse = Assert.IsType<BaseResponse<UserResponse>>(okResult.Value);
            Assert.True(baseResponse.Success);
            Assert.Equal(response.UserId, baseResponse.Data.UserId);
            Assert.Equal(response.Name, baseResponse.Data.Name);
        }

        #endregion

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_ShouldReturnOk_WhenUsersExist()
        {
            // Arrange
            var users = new List<UserResponse>
            {
                new() { 
                    UserId = 1, 
                    Name = "User 1", 
                    Email = "user1@example.com",
                    Active = true,
                    Type = TypeUser.User
                },
                new() { 
                    UserId = 2, 
                    Name = "User 2", 
                    Email = "user2@example.com",
                    Active = true,
                    Type = TypeUser.Admin
                }
            };

            _usersServiceMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(users);

            // Act
            var result = await _controller.GetAllAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var baseResponse = Assert.IsType<BaseResponse<List<UserResponse>>>(okResult.Value);
            Assert.True(baseResponse.Success);
            Assert.Equal(2, baseResponse.Data.Count);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnOk_WhenNoUsers()
        {
            // Arrange
            var emptyUsers = new List<UserResponse>();

            _usersServiceMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(emptyUsers);

            // Act
            var result = await _controller.GetAllAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var baseResponse = Assert.IsType<BaseResponse<List<UserResponse>>>(okResult.Value);
            Assert.True(baseResponse.Success);
            Assert.Empty(baseResponse.Data);
        }

        #endregion

        #region GetGamesByUserAsync Tests

        [Fact]
        public async Task GetGamesByUserAsync_ShouldReturnOk_WhenUserHasGames()
        {
            // Arrange
            var userId = 1;
            var claims = new List<Claim>
            {
                new("id", userId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "test");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext.HttpContext.User = principal;

            var userGames = new List<UserLibraryGameResponse>
            {
                new() {
                    GameId = 1,
                    Name = "Game 1",
                    Genre = "Action",
                    OriginalPrice = 60M,
                    PricePaid = 45M,
                    OriginalCurrency = "BRL",
                    PurchaseCurrency = "BRL",
                    PurchaseDate = DateTime.UtcNow
                }
            };

            _usersServiceMock
                .Setup(x => x.GetGamesByUserAsync(userId))
                .ReturnsAsync(userGames);

            // Act
            var result = await _controller.GetGamesByUserAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var baseResponse = Assert.IsType<BaseResponse<List<UserLibraryGameResponse>>>(okResult.Value);
            Assert.True(baseResponse.Success);
            Assert.Single(baseResponse.Data);
            Assert.Equal("Game 1", baseResponse.Data.First().Name);
        }

        [Fact]
        public async Task GetGamesByUserAsync_ShouldReturnNoContent_WhenUserHasNoGames()
        {
            // Arrange
            var userId = 1;
            var claims = new List<Claim>
            {
                new Claim("id", userId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "test");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext.HttpContext.User = principal;

            _usersServiceMock
                .Setup(x => x.GetGamesByUserAsync(userId))
                .ReturnsAsync((List<UserLibraryGameResponse>?)null);

            // Act
            var result = await _controller.GetGamesByUserAsync();

            // Assert
            var noContentResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, noContentResult.StatusCode);
        }

        #endregion

        #region Service Method Verification Tests

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(999)]
        public async Task GetAsync_ShouldCallServiceWithCorrectId(int userId)
        {
            // Arrange
            var response = new UserResponse { UserId = userId };
            _usersServiceMock
                .Setup(x => x.GetAsync(userId))
                .ReturnsAsync(response);

            // Act
            await _controller.GetAsync(userId);

            // Assert
            _usersServiceMock.Verify(x => x.GetAsync(userId), Times.Once);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(100)]
        public async Task DeleteAsync_ShouldCallServiceWithCorrectId(int userId)
        {
            // Arrange
            var response = BaseResponse<object>.Ok(null);
            _usersServiceMock
                .Setup(x => x.DeleteAsync(userId))
                .ReturnsAsync(response);

            // Act
            await _controller.DeleteAsync(userId);

            // Assert
            _usersServiceMock.Verify(x => x.DeleteAsync(userId), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldCallServiceWithCorrectParameters()
        {
            // Arrange
            var userId = 5;
            var request = new UpdateUserRequest { Name = "Updated" };
            var response = BaseResponse<object>.Ok(null);

            _usersServiceMock
                .Setup(x => x.UpdateAsync(userId, request))
                .ReturnsAsync(response);

            // Act
            await _controller.UpdateAsync(userId, request);

            // Assert
            _usersServiceMock.Verify(x => x.UpdateAsync(userId, request), Times.Once);
        }

        #endregion
    }
}
