using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using PhotosMarket.API.Services;
using PhotosMarket.API.Repositories;
using PhotosMarket.API.Configuration;
using PhotosMarket.API.Models;
using PhotosMarket.API.DTOs;
using System.IdentityModel.Tokens.Jwt;

namespace PhotosMarket.API.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPhotographerSettingsRepository> _photographerSettingsRepositoryMock;
    private readonly Mock<ILogger<AuthService>> _loggerMock;
    private readonly AuthService _authService;
    private readonly GooglePhotosSettings _googleSettings;
    private readonly JwtSettings _jwtSettings;

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _photographerSettingsRepositoryMock = new Mock<IPhotographerSettingsRepository>();
        _loggerMock = new Mock<ILogger<AuthService>>();

        _googleSettings = new GooglePhotosSettings
        {
            ClientId = "test-client-id",
            ClientSecret = "test-client-secret",
            RedirectUri = "http://localhost:3000/callback"
        };

        _jwtSettings = new JwtSettings
        {
            Secret = "this-is-a-very-secure-secret-key-for-testing-at-least-32-characters-long",
            Issuer = "test-issuer",
            Audience = "test-audience",
            ExpiryMinutes = 60
        };

        _authService = new AuthService(
            _userRepositoryMock.Object,
            _photographerSettingsRepositoryMock.Object,
            Options.Create(_googleSettings),
            Options.Create(_jwtSettings),
            _loggerMock.Object
        );
    }

    [Fact]
    public void GenerateJwtToken_ValidUser_ReturnsValidToken()
    {
        // Arrange
        var user = new User
        {
            Id = "user123",
            GoogleUserId = "google123",
            Email = "test@example.com",
            Name = "Test User",
            Role = UserRole.Customer
        };

        // Act
        var token = _authService.GenerateJwtToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.Id);
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == user.Email);
        jwtToken.Claims.Should().Contain(c => c.Type == "role" && c.Value == user.Role.ToString());
    }

    [Theory]
    [InlineData("12345678-9", true)]
    [InlineData("11111111-1", true)]
    [InlineData("12345678-0", false)]
    [InlineData("invalid", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void ValidateRut_VariousInputs_ReturnsExpectedResult(string? rut, bool expectedResult)
    {
        // Act
        var result = _authService.ValidateRut(rut ?? "");

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public async Task AuthenticateAdminAsync_ValidUsername_ReturnsAuthResponse()
    {
        // Arrange
        var username = "admin";
        var existingUser = new User
        {
            Id = "admin123",
            GoogleUserId = "admin-google-id",
            Email = "admin@photosmarket.com",
            Name = "Admin User",
            Role = UserRole.Admin,
            IsRegistrationComplete = true
        };

        _userRepositoryMock
            .Setup(r => r.GetByGoogleUserIdAsync("admin-google-id"))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _authService.AuthenticateAdminAsync(username);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().NotBeNullOrEmpty();
        result.User.Should().NotBeNull();
        result.User.Email.Should().Be("admin@photosmarket.com");
        result.User.Role.Should().Be("Admin");
    }

    [Fact]
    public async Task CompleteRegistrationAsync_ValidRequest_UpdatesUserSuccessfully()
    {
        // Arrange
        var userId = "user123";
        var existingUser = new User
        {
            Id = userId,
            GoogleUserId = "google123",
            Email = "test@example.com",
            Name = "Test User",
            Role = UserRole.Customer,
            IsRegistrationComplete = false
        };

        var request = new CompleteRegistrationRequest
        {
            FullName = "John Doe",
            Rut = "12345678-9",
            Phone = "+56912345678",
            Address = "Test Address 123"
        };

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(existingUser);

        _userRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _authService.CompleteRegistrationAsync(userId, request);

        // Assert
        result.Should().NotBeNull();
        result.User.FullName.Should().Be("John Doe");
        result.User.Rut.Should().Be("12345678-9");
        result.User.Phone.Should().Be("+56912345678");
        _userRepositoryMock.Verify(r => r.UpdateAsync(It.Is<User>(u => 
            u.Id == userId && 
            u.IsRegistrationComplete == true &&
            u.FullName == "John Doe"
        )), Times.Once);
    }

    [Fact]
    public async Task CompleteRegistrationAsync_InvalidRut_ThrowsException()
    {
        // Arrange
        var userId = "user123";
        var existingUser = new User
        {
            Id = userId,
            GoogleUserId = "google123",
            Email = "test@example.com",
            Name = "Test User",
            Role = UserRole.Customer,
            IsRegistrationComplete = false
        };

        var request = new CompleteRegistrationRequest
        {
            FullName = "John Doe",
            Rut = "invalid-rut",
            Phone = "+56912345678",
            Address = "Test Address 123"
        };

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(existingUser);

        // Act
        Func<Task> act = async () => await _authService.CompleteRegistrationAsync(userId, request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("RUT inválido");
    }

    [Fact]
    public async Task CompleteRegistrationAsync_UserNotFound_ThrowsException()
    {
        // Arrange
        var userId = "nonexistent";
        var request = new CompleteRegistrationRequest
        {
            FullName = "John Doe",
            Rut = "12345678-9",
            Phone = "+56912345678",
            Address = "Test Address 123"
        };

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        Func<Task> act = async () => await _authService.CompleteRegistrationAsync(userId, request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Usuario no encontrado");
    }
}
