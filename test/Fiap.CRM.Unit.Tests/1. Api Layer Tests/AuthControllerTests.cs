using Fiap.CRM.Api.Controllers;
using Fiap.CRM.Application.Auth.Models.Request;
using Fiap.CRM.Application.Auth.Models.Response;
using Fiap.CRM.Application.Auth.Services;
using Fiap.CRM.Application.Common;
using Fiap.CRM.Domain.SeedWork;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Fiap.CRM.Unit.Tests._1._Api_Layer_Tests;

public class AuthControllerTests
{
    readonly Mock<IAuthService> _authServiceMock;
    readonly Mock<INotification> _notificationMock;
    readonly AuthController _controller;

    public AuthControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _notificationMock = new Mock<INotification>();
        _controller = new AuthController(_authServiceMock.Object, _notificationMock.Object);
    }

    #region Login Tests

    [Fact]
    public async Task LoginAsync_ShouldReturnOk_WhenServiceReturnsSuccess()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "testuser@hotmail.com",
            Password = "password#123"
        };

        var response = new LoginResponse
        {
            Token = "sample.jwt.token"
        };

        _authServiceMock
            .Setup(x => x.LoginAsync(request))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.LoginAsync(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var baseResponse = Assert.IsType<BaseResponse<LoginResponse>>(okResult.Value);
        Assert.True(baseResponse.Success);
        Assert.Equal(response, baseResponse.Data);
        Assert.Equal("sample.jwt.token", baseResponse.Data.Token);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnBadRequest_WhenServiceReturnsNull()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "invalid@example.com",
            Password = "wrongpassword"
        };

        _authServiceMock
            .Setup(x => x.LoginAsync(request))
            .ReturnsAsync((LoginResponse?)null);

        // Act
        var result = await _controller.LoginAsync(request);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(204, noContentResult.StatusCode);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnOk_WhenServiceReturnsEmptyToken()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "user@example.com",
            Password = "password123"
        };

        var response = new LoginResponse
        {
            Token = string.Empty
        };

        _authServiceMock
            .Setup(x => x.LoginAsync(request))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.LoginAsync(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Theory]
    [InlineData("user1@test.com", "pass1", "token1")]
    [InlineData("user2@test.com", "pass2", "token2")]
    [InlineData("admin@company.com", "adminpass", "admintoken")]
    public async Task LoginAsync_ShouldReturnExpectedToken_ForVariousValidCredentials(
        string username, string password, string expectedToken)
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = username,
            Password = password
        };

        var response = new LoginResponse
        {
            Token = expectedToken
        };

        _authServiceMock
            .Setup(x => x.LoginAsync(It.Is<LoginRequest>(r => r.Username == username && r.Password == password)))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.LoginAsync(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var baseResponse = Assert.IsType<BaseResponse<LoginResponse>>(okResult.Value);
        Assert.True(baseResponse.Success);
        Assert.Equal(expectedToken, baseResponse.Data.Token);
    }

    [Fact]
    public async Task LoginAsync_ShouldCallServiceOnce()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "test@example.com",
            Password = "testpass"
        };

        var response = new LoginResponse { Token = "testtoken" };

        _authServiceMock
            .Setup(x => x.LoginAsync(request))
            .ReturnsAsync(response);

        // Act
        await _controller.LoginAsync(request);

        // Assert
        _authServiceMock.Verify(x => x.LoginAsync(request), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnNoContent_WhenNullRequest()
    {
        // Arrange
        LoginRequest? request = null;

        _authServiceMock
            .Setup(x => x.LoginAsync(It.IsAny<LoginRequest>()))
            .ReturnsAsync((LoginResponse?)null);

        // Act
        var result = await _controller.LoginAsync(request!);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(204, noContentResult.StatusCode);
    }

    [Theory]
    [InlineData(null, "password")]
    [InlineData("", "password")]
    [InlineData("username", null)]
    [InlineData("username", "")]
    public async Task LoginAsync_ShouldReturnNoContent_WhenInvalidRequestFields(string username, string password)
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = username,
            Password = password
        };

        _authServiceMock
            .Setup(x => x.LoginAsync(request))
            .ReturnsAsync((LoginResponse?)null);

        // Act
        var result = await _controller.LoginAsync(request);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(204, noContentResult.StatusCode);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnCorrectStatusCode_WhenSuccess()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "success@test.com",
            Password = "successpass"
        };

        var response = new LoginResponse { Token = "successtoken" };

        _authServiceMock
            .Setup(x => x.LoginAsync(request))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.LoginAsync(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnCorrectStatusCode_WhenFailure()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "fail@test.com",
            Password = "failpass"
        };

        _authServiceMock
            .Setup(x => x.LoginAsync(request))
            .ReturnsAsync((LoginResponse?)null);

        // Act
        var result = await _controller.LoginAsync(request);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(204, noContentResult.StatusCode);
    }

    #endregion
}