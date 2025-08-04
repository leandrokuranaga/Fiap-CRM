using Fiap.CRM.Api.Controllers;
using Fiap.CRM.Application.Auth.Models.Request;
using Fiap.CRM.Application.Auth.Models.Response;
using Fiap.CRM.Application.Auth.Services;
using Fiap.CRM.Application.Common;
using Fiap.CRM.Domain.SeedWork;
using Microsoft.AspNetCore.Mvc;

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


    #region Login
    [Fact]
    public async Task Login_ShouldReturnOk_WhenServiceReturnsSuccess()
    {
        #region Arrange
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
        #endregion

        #region Act
        var result = await _controller.LoginAsync(request);
        #endregion

        #region Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var baseResponse = Assert.IsType<BaseResponse<LoginResponse>>(okResult.Value);
        Assert.True(baseResponse.Success);
        Assert.Equal(response, baseResponse.Data);
        #endregion
    }
    #endregion
}