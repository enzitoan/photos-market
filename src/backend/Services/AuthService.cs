using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PhotosMarket.API.Configuration;
using PhotosMarket.API.Models;
using PhotosMarket.API.Repositories;
using PhotosMarket.API.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PhotosMarket.API.Services;

public interface IAuthService
{
    Task<AuthResponse> AuthenticateWithGoogleAsync(string idToken, string? refreshToken = null, string? accessToken = null);
    Task<AuthResponse> AuthenticatePhotographerWithGoogleAsync(string idToken, string? refreshToken = null, string? accessToken = null);
    Task<AuthResponse> AuthenticateAdminAsync(string username);
    string GenerateJwtToken(User user);
    Task<User?> GetUserFromTokenAsync(string token);
}

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPhotographerSettingsRepository _photographerSettingsRepository;
    private readonly GooglePhotosSettings _googleSettings;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthService> _logger;
    private const string PHOTOGRAPHER_EMAIL = "ahumada.enzo@gmail.com";

    public AuthService(
        IUserRepository userRepository,
        IPhotographerSettingsRepository photographerSettingsRepository,
        IOptions<GooglePhotosSettings> googleSettings,
        IOptions<JwtSettings> jwtSettings,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _photographerSettingsRepository = photographerSettingsRepository;
        _googleSettings = googleSettings.Value;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }

    public async Task<AuthResponse> AuthenticateWithGoogleAsync(string idToken, string? refreshToken = null, string? accessToken = null)
    {
        // Verify Google ID token
        var payload = await GoogleJsonWebSignature.ValidateAsync(idToken);

        _logger.LogInformation("Authenticating user with email: {Email}", payload.Email);

        // Determinar el rol basado en el email
        var role = payload.Email.Equals(PHOTOGRAPHER_EMAIL, StringComparison.OrdinalIgnoreCase) 
            ? UserRole.Photographer 
            : UserRole.Customer;

        // Si es el fotógrafo, guardar tokens en PhotographerSettings también
        if (role == UserRole.Photographer && !string.IsNullOrEmpty(refreshToken))
        {
            _logger.LogInformation("Saving photographer tokens to PhotographerSettings");
            var settings = await _photographerSettingsRepository.GetSettingsAsync() ?? new PhotographerSettings();
            settings.PhotographerGoogleRefreshToken = refreshToken;
            settings.PhotographerGoogleAccessToken = accessToken;
            settings.PhotographerTokenExpiresAt = accessToken != null ? DateTime.UtcNow.AddHours(1) : null;
            await _photographerSettingsRepository.UpdateSettingsAsync(settings);
            _logger.LogInformation("Photographer tokens saved successfully");
        }
        else if (role == UserRole.Photographer && string.IsNullOrEmpty(refreshToken))
        {
            _logger.LogWarning("Photographer authentication without refresh token - Google Photos access will not be available");
        }

        // Check if user exists
        var existingUser = await _userRepository.GetByGoogleUserIdAsync(payload.Subject);

        User user;
        
        if (existingUser == null)
        {
            // Create new user
            user = new User
            {
                GoogleUserId = payload.Subject,
                Email = payload.Email,
                Name = payload.Name,
                Role = role,
                GoogleRefreshToken = refreshToken,
                GoogleAccessToken = accessToken,
                GoogleTokenExpiresAt = accessToken != null ? DateTime.UtcNow.AddHours(1) : null,
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
            };

            user = await _userRepository.CreateAsync(user);
        }
        else
        {
            // Update last login and refresh token if provided
            existingUser.LastLoginAt = DateTime.UtcNow;
            existingUser.Role = role; // Actualizar rol si cambió
            if (!string.IsNullOrEmpty(refreshToken))
            {
                existingUser.GoogleRefreshToken = refreshToken;
            }
            if (!string.IsNullOrEmpty(accessToken))
            {
                existingUser.GoogleAccessToken = accessToken;
                existingUser.GoogleTokenExpiresAt = DateTime.UtcNow.AddHours(1);
            }
            user = await _userRepository.UpdateAsync(existingUser);
        }

        var token = GenerateJwtToken(user);

        return new AuthResponse
        {
            Token = token,
            UserId = user.Id,
            Email = user.Email,
            Name = user.Name,
            IsAdmin = user.IsAdmin
        };
    }

    public async Task<AuthResponse> AuthenticatePhotographerWithGoogleAsync(string idToken, string? refreshToken = null, string? accessToken = null)
    {
        // Verify Google ID token
        var payload = await GoogleJsonWebSignature.ValidateAsync(idToken);

        _logger.LogInformation("Authenticating photographer with email: {Email}, RefreshToken present: {HasRefreshToken}", 
            payload.Email, 
            !string.IsNullOrEmpty(refreshToken));

        // Verificar que sea el email del fotógrafo
        if (!payload.Email.Equals(PHOTOGRAPHER_EMAIL, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Solo el fotógrafo puede autenticarse con esta cuenta");
        }

        // Guardar tokens en PhotographerSettings
        var settings = await _photographerSettingsRepository.GetSettingsAsync() ?? new PhotographerSettings();
        settings.PhotographerGoogleRefreshToken = refreshToken;
        settings.PhotographerGoogleAccessToken = accessToken;
        settings.PhotographerTokenExpiresAt = accessToken != null ? DateTime.UtcNow.AddHours(1) : null;
        await _photographerSettingsRepository.UpdateSettingsAsync(settings);

        _logger.LogInformation("Photographer tokens saved to settings. RefreshToken saved: {HasRefreshToken}", 
            !string.IsNullOrEmpty(settings.PhotographerGoogleRefreshToken));

        // Autenticar usuario con rol de fotógrafo
        return await AuthenticateWithGoogleAsync(idToken, refreshToken, accessToken);
    }

    public async Task<AuthResponse> AuthenticateAdminAsync(string username)
    {
        // Buscar o crear usuario admin con email especial
        var adminEmail = $"{username}@photosmarket.admin";
        var existingUser = await _userRepository.GetByEmailAsync(adminEmail);

        User user;

        if (existingUser == null)
        {
            // Crear nuevo usuario admin
            user = new User
            {
                GoogleUserId = $"admin-{username}", // Identificador único para admin
                Email = adminEmail,
                Name = "Administrador",
                Role = UserRole.Admin,
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
            };

            user = await _userRepository.CreateAsync(user);
        }
        else
        {
            // Actualizar último login
            existingUser.LastLoginAt = DateTime.UtcNow;
            existingUser.Role = UserRole.Admin; // Asegurar que tenga rol de admin
            user = await _userRepository.UpdateAsync(existingUser);
        }

        var token = GenerateJwtToken(user);

        return new AuthResponse
        {
            Token = token,
            UserId = user.Id,
            Email = user.Email,
            Name = user.Name,
            IsAdmin = user.IsAdmin
        };
    }

    public string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim("googleUserId", user.GoogleUserId),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Agregar claim "Admin" para usuarios con rol Admin o Photographer
        if (user.IsAdmin)
        {
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public async Task<User?> GetUserFromTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return null;

            return await _userRepository.GetByIdAsync(userId);
        }
        catch
        {
            return null;
        }
    }
}
