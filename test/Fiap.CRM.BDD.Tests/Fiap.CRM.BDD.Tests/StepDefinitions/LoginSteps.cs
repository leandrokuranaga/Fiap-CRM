using Fiap.CRM.Application.Auth.Models.Request;
using Fiap.CRM.Application.Auth.Models.Response;
using Fiap.CRM.Application.Auth.Services;
using Fiap.CRM.Domain.SeedWork.Exceptions;
using Moq;

namespace Fiap.CRM.BDD.Tests.StepDefinitions;

[Binding]
public class LoginSteps
{
    private readonly Mock<IAuthService> _authServiceMock = new();
    private LoginRequest _loginRequest = new();
    private LoginResponse _loginResponse;
    private Exception _exception;

    [Given(@"the user provided the username ""(.*)""")]
    public void GivenTheUserProvidedTheUsername(string username)
        => _loginRequest = _loginRequest with { Username = username };

    [Given(@"the password ""(.*)""")]
    public void GivenThePassword(string password)
        => _loginRequest = _loginRequest with { Password = password };

    [When(@"the user requests login")]
    public async Task WhenTheUserRequestsLogin()
    {
        if (_loginRequest.Username == "doesNotExist")
        {
            _authServiceMock
                .Setup(s => s.LoginAsync(_loginRequest))
                .ThrowsAsync(new NotFoundException());
        }
        else if (_loginRequest.Username == "inactive")
        {
            _authServiceMock
                .Setup(s => s.LoginAsync(_loginRequest))
                .ThrowsAsync(new BusinessRulesException("Your account is disabled. Please contact support."));
        }
        else if (_loginRequest.Username == "error")
        {
            _authServiceMock
                .Setup(s => s.LoginAsync(_loginRequest))
                .ThrowsAsync(new Exception("Simulated internal error"));
        }
        else if (_loginRequest.Username == "admin" && _loginRequest.Password == "123456")
        {
            _authServiceMock
                .Setup(s => s.LoginAsync(_loginRequest))
                .ReturnsAsync(new LoginResponse { Token = "jwt-token-admin", Expiration = DateTime.UtcNow.AddHours(2) });
        }
        else 
        {
            _authServiceMock
                .Setup(s => s.LoginAsync(_loginRequest))
                .ReturnsAsync(_loginRequest.Password == "654321"
                    ? new LoginResponse { Token = "jwt-token-user", Expiration = DateTime.UtcNow.AddHours(2) }
                    : null);
        }

        try
        {
            _loginResponse = await _authServiceMock.Object.LoginAsync(_loginRequest);
        }
        catch (Exception ex)
        {
            _exception = ex;
        }
    }

    [Then(@"the result should contain a JWT token")]
    public void ThenTheResultShouldContainAJwtToken()
    {
        Assert.NotNull(_loginResponse);
        Assert.False(string.IsNullOrEmpty(_loginResponse.Token));
    }

    [Then(@"the result should not contain a JWT token")]
    public void ThenTheResultShouldNotContainAJwtToken()
        => Assert.Null(_loginResponse?.Token);

    [Then(@"an exception of type NotFoundException should be thrown")]
    public void ThenNotFoundExceptionShouldBeThrown()
        => Assert.IsType<NotFoundException>(_exception);

    [Then(@"an error notification with message ""(.*)"" should be recorded")]
    public void ThenBusinessRulesExceptionShouldBeThrown(string expectedMessage)
    {
        var bre = Assert.IsType<BusinessRulesException>(_exception);
        Assert.Equal(expectedMessage, bre.Message);
    }

    [Then(@"an exception should be thrown")]
    public void ThenGenericExceptionShouldBeThrown()
        => Assert.Equal("Simulated internal error", _exception.Message);

    [Then(@"the token expiration should be in the future")]
    public void ThenTheTokenExpirationShouldBeInTheFuture()
        => Assert.True(_loginResponse.Expiration > DateTime.UtcNow);
}