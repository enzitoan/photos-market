using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
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
    Task<AuthResponse> RegisterManualAsync(ManualRegisterRequest request);
    Task<AuthResponse> LoginManualAsync(ManualLoginRequest request);
    Task<AuthResponse> CompleteRegistrationAsync(string userId, CompleteRegistrationRequest request);
    string GenerateJwtToken(User user);
    Task<User?> GetUserFromTokenAsync(string token);
    bool ValidateRut(string rut);
}

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPhotographerSettingsRepository _photographerSettingsRepository;
    private readonly GooglePhotosSettings _googleSettings;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthService> _logger;
    private readonly PasswordHasher<User> _passwordHasher;
    private static readonly string[] PHOTOGRAPHER_EMAILS = { "egan.fotografia.ph@gmail.com" };

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
        _passwordHasher = new PasswordHasher<User>();
    }

    public async Task<AuthResponse> AuthenticateWithGoogleAsync(string idToken, string? refreshToken = null, string? accessToken = null)
    {
        // Verify Google ID token
        var payload = await GoogleJsonWebSignature.ValidateAsync(idToken);

        _logger.LogInformation("Authenticating user with email: {Email}", payload.Email);

        // Determinar el rol basado en el email
        var role = PHOTOGRAPHER_EMAILS.Contains(payload.Email, StringComparer.OrdinalIgnoreCase)
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
        bool needsRegistration = false;
        
        if (existingUser == null)
        {
            // Create new user (incomplete registration)
            user = new User
            {
                GoogleUserId = payload.Subject,
                Email = payload.Email,
                Name = payload.Name,
                Role = role,
                GoogleRefreshToken = refreshToken,
                GoogleAccessToken = accessToken,
                GoogleTokenExpiresAt = accessToken != null ? DateTime.UtcNow.AddHours(1) : null,
                IsRegistrationComplete = role == UserRole.Photographer, // Los fotógrafos no necesitan completar registro
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
            };

            user = await _userRepository.CreateAsync(user);
            needsRegistration = !user.IsRegistrationComplete;
            
            _logger.LogInformation("New user created. NeedsRegistration: {NeedsRegistration}", needsRegistration);
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
            
            // Si es fotógrafo, marcar registro como completo automáticamente
            if (role == UserRole.Photographer)
            {
                existingUser.IsRegistrationComplete = true;
            }
            
            user = await _userRepository.UpdateAsync(existingUser);
            needsRegistration = !user.IsRegistrationComplete;
            
            _logger.LogInformation("Existing user updated. NeedsRegistration: {NeedsRegistration}", needsRegistration);
        }

        var token = GenerateJwtToken(user);

        return new AuthResponse
        {
            Token = needsRegistration ? string.Empty : token,
            TempToken = needsRegistration ? token : null,
            UserId = user.Id,
            Email = user.Email,
            Name = user.Name,
            IsAdmin = user.IsAdmin,
            NeedsRegistration = needsRegistration
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
        if (!PHOTOGRAPHER_EMAILS.Contains(payload.Email, StringComparer.OrdinalIgnoreCase))
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

    public async Task<AuthResponse> RegisterManualAsync(ManualRegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("El nombre es requerido");

        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ArgumentException("El correo electrónico es requerido");

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
            throw new ArgumentException("La contraseña debe tener al menos 8 caracteres");

        if (!string.Equals(request.Password, request.ConfirmPassword, StringComparison.Ordinal))
            throw new ArgumentException("Las contraseñas no coinciden");

        if (string.IsNullOrWhiteSpace(request.Phone))
            throw new ArgumentException("El teléfono es requerido");

        if (string.IsNullOrWhiteSpace(request.IdType) || (request.IdType != "RUT" && request.IdType != "DNI"))
            throw new ArgumentException("Tipo de identificación inválido. Debe ser 'RUT' o 'DNI'");

        if (string.IsNullOrWhiteSpace(request.IdNumber))
            throw new ArgumentException("El número de identificación es requerido");

        if (request.IdType == "RUT" && !ValidateRut(request.IdNumber))
            throw new ArgumentException("RUT inválido. Por favor verifica el formato (ej: 12345678-9)");

        var age = DateTime.Today.Year - request.BirthDate.Year;
        if (request.BirthDate.Date > DateTime.Today.AddYears(-age)) age--;

        if (age < 18)
            throw new ArgumentException("Debes ser mayor de 18 años para registrarte");

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var existingUser = await _userRepository.GetByEmailAsync(normalizedEmail);
        if (existingUser != null)
            throw new InvalidOperationException("Ya existe un usuario registrado con ese correo electrónico");

        var user = new User
        {
            GoogleUserId = $"manual-{normalizedEmail}",
            Email = normalizedEmail,
            Name = request.Name.Trim(),
            Role = UserRole.Customer,
            Phone = request.Phone.Trim(),
            IdType = request.IdType,
            IdNumber = request.IdNumber.Trim(),
            BirthDate = request.BirthDate.Date,
            IsRegistrationComplete = true,
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
        user = await _userRepository.CreateAsync(user);

        _logger.LogInformation("Manual user registered successfully with email {Email}", normalizedEmail);

        var token = GenerateJwtToken(user);

        return new AuthResponse
        {
            Token = token,
            UserId = user.Id,
            Email = user.Email,
            Name = user.Name,
            IsAdmin = user.IsAdmin,
            NeedsRegistration = false
        };
    }

    public async Task<AuthResponse> LoginManualAsync(ManualLoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            throw new ArgumentException("El correo electrónico y la contraseña son requeridos");

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(normalizedEmail);
        if (user == null || string.IsNullOrWhiteSpace(user.PasswordHash))
            throw new UnauthorizedAccessException("Credenciales inválidas");

        var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verificationResult == PasswordVerificationResult.Failed)
            throw new UnauthorizedAccessException("Credenciales inválidas");

        user.LastLoginAt = DateTime.UtcNow;
        user = await _userRepository.UpdateAsync(user);

        var token = GenerateJwtToken(user);

        return new AuthResponse
        {
            Token = token,
            UserId = user.Id,
            Email = user.Email,
            Name = user.Name,
            IsAdmin = user.IsAdmin,
            NeedsRegistration = false
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

    public async Task<AuthResponse> CompleteRegistrationAsync(string userId, CompleteRegistrationRequest request)
    {
        // Obtener usuario
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new ArgumentException("Usuario no encontrado");
        }

        // Validar que no haya completado ya el registro
        if (user.IsRegistrationComplete)
        {
            throw new InvalidOperationException("El usuario ya completó el registro");
        }

        // Validar tipo de identificación
        if (request.IdType != "RUT" && request.IdType != "DNI")
        {
            throw new ArgumentException("Tipo de identificación inválido. Debe ser 'RUT' o 'DNI'");
        }

        // Validar RUT chileno solo si el tipo es RUT
        if (request.IdType == "RUT" && !ValidateRut(request.IdNumber))
        {
            throw new ArgumentException("RUT inválido. Por favor verifica el formato (ej: 12345678-9)");
        }

        // Validar que el DNI no esté vacío
        if (string.IsNullOrWhiteSpace(request.IdNumber))
        {
            throw new ArgumentException("El número de identificación es requerido");
        }

        // Validar fecha de nacimiento (mayor de 18 años)
        var age = DateTime.Today.Year - request.BirthDate.Year;
        if (request.BirthDate.Date > DateTime.Today.AddYears(-age)) age--;
        
        if (age < 18)
        {
            throw new ArgumentException("Debes ser mayor de 18 años para registrarte");
        }

        // Actualizar usuario
        user.Phone = request.Phone;
        user.IdType = request.IdType;
        user.IdNumber = request.IdNumber;
        user.BirthDate = request.BirthDate;
        user.IsRegistrationComplete = true;

        user = await _userRepository.UpdateAsync(user);

        _logger.LogInformation("User {UserId} completed registration successfully with {IdType}", userId, request.IdType);

        // Generar nuevo token
        var token = GenerateJwtToken(user);

        return new AuthResponse
        {
            Token = token,
            UserId = user.Id,
            Email = user.Email,
            Name = user.Name,
            IsAdmin = user.IsAdmin,
            NeedsRegistration = false
        };
    }

    public bool ValidateRut(string rut)
    {
        try
        {
            // Eliminar puntos y guiones
            rut = rut.Replace(".", "").Replace("-", "").Trim().ToUpper();

            // Verificar largo (mínimo 8 caracteres: 7 números + 1 dígito verificador)
            if (rut.Length < 8 || rut.Length > 9)
                return false;

            // Separar número y dígito verificador
            var rutNumber = rut.Substring(0, rut.Length - 1);
            var dvProvided = rut.Substring(rut.Length - 1);

            // Verificar que el número sea numérico
            if (!int.TryParse(rutNumber, out int rutNum))
                return false;

            // Calcular dígito verificador
            int sum = 0;
            int multiplier = 2;

            for (int i = rutNumber.Length - 1; i >= 0; i--)
            {
                sum += int.Parse(rutNumber[i].ToString()) * multiplier;
                multiplier = multiplier == 7 ? 2 : multiplier + 1;
            }

            int dvExpected = 11 - (sum % 11);
            string dvCalculated = dvExpected == 11 ? "0" : dvExpected == 10 ? "K" : dvExpected.ToString();

            return dvProvided == dvCalculated;
        }
        catch
        {
            return false;
        }
    }
}
