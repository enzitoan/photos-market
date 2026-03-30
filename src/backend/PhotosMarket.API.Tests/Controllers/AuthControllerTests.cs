using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PhotosMarket.API.Controllers;
using PhotosMarket.API.Services;
using PhotosMarket.API.DTOs;

namespace PhotosMarket.API.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<IGoogleOAuthService> _googleOAuthServiceMock;
    private readonly Mock<ILogger<AuthController>> _loggerMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _googleOAuthServiceMock = new Mock<IGoogleOAuthService>();
        _loggerMock = new Mock<ILogger<AuthController>>();

        _controller = new AuthController(
            _authServiceMock.Object,
            _googleOAuthServiceMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public void GoogleLogin_ReturnsOkWithAuthUrl()
    {
        // Arrange
        var expectedAuthUrl = "https://accounts.google.com/o/oauth2/auth?client_id=test";
        _googleOAuthServiceMock
            .Setup(s => s.GetAuthorizationUrl(It.IsAny<string>()))
            .Returns(expectedAuthUrl);

        // Act
        var result = _controller.GoogleLogin();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        _googleOAuthServiceMock.Verify(s => s.GetAuthorizationUrl(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task GoogleCallback_WithNullCode_ReturnsBadRequest()
    {
        // Arrange
        var request = new GoogleAuthCallbackRequest
        {
            Code = null
        };

        // Act
        var result = await _controller.GoogleCallback(request);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
        var response = badRequestResult.Value as ApiResponse<AuthResponse>;
        response.Should().NotBeNull();
        response!.Success.Should().BeFalse();
        response.Message.Should().Contain("missing");
    }

    [Fact]
    public async Task GoogleCallback_WithEmptyCode_ReturnsBadRequest()
    {
        // Arrange
        var request = new GoogleAuthCallbackRequest
        {
            Code = string.Empty
        };

        // Act
        var result = await _controller.GoogleCallback(request);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task GoogleCallback_ValidCode_ReturnsOkWithAuthResponse()
    {
        // Arrange
        var request = new GoogleAuthCallbackRequest
        {
            Code = "valid-auth-code"
        };

        var mockTokenResponse = new GoogleTokenResponse
        {
            IdToken = "valid-id-token",
            RefreshToken = "valid-refresh-token",
            AccessToken = "valid-access-token"
        };

        var mockAuthResponse = new AuthResponse
        {
            Token = "jwt-token",
            User = new UserDto
            {
                Id = "user123",
                Email = "test@example.com",
                Name = "Test User",
                Role = "Customer"
            },
            NeedsRegistration = false
        };

        _googleOAuthServiceMock
            .Setup(s => s.ExchangeCodeForTokensAsync(request.Code))
            .ReturnsAsync(mockTokenResponse);

        _authServiceMock
            .Setup(s => s.AuthenticateWithGoogleAsync(
                mockTokenResponse.IdToken,
                mockTokenResponse.RefreshToken,
                mockTokenResponse.AccessToken))
            .ReturnsAsync(mockAuthResponse);

        // Act
        var result = await _controller.GoogleCallback(request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        var response = okResult.Value as ApiResponse<AuthResponse>;
        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.Token.Should().Be("jwt-token");
    }
}
