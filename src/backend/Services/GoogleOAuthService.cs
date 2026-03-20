using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Microsoft.Extensions.Options;
using PhotosMarket.API.Configuration;

namespace PhotosMarket.API.Services;

/// <summary>
/// Servicio para autenticación OAuth de Google (para clientes y fotógrafo)
/// Las fotos se obtienen de Google Drive mediante GoogleDriveService
/// </summary>
public interface IGoogleOAuthService
{
    string GetAuthorizationUrl(string state);
    Task<GoogleTokenResponse> ExchangeCodeForTokenAsync(string code);
}

public class GoogleTokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string? IdToken { get; set; }
    public string? RefreshToken { get; set; }
    public long ExpiresInSeconds { get; set; }
}

public class GoogleOAuthService : IGoogleOAuthService
{
    private readonly GooglePhotosSettings _settings;
    private readonly ILogger<GoogleOAuthService> _logger;

    public GoogleOAuthService(
        IOptions<GooglePhotosSettings> settings,
        ILogger<GoogleOAuthService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Genera la URL de autorización de Google OAuth
    /// </summary>
    public string GetAuthorizationUrl(string state)
    {
        // Log configuration check
        _logger.LogInformation("🔍 GoogleOAuth Settings Check:");
        _logger.LogInformation("  - ClientId: {ClientId}", string.IsNullOrEmpty(_settings.ClientId) ? "EMPTY" : _settings.ClientId.Substring(0, Math.Min(20, _settings.ClientId.Length)) + "...");
        _logger.LogInformation("  - RedirectUri: {RedirectUri}", _settings.RedirectUri);
        _logger.LogInformation("  - Scopes Count: {ScopesCount}", _settings.Scopes?.Count ?? 0);
        
        if (_settings.Scopes != null && _settings.Scopes.Any())
        {
            foreach (var scope in _settings.Scopes)
            {
                _logger.LogInformation("    - Scope: {Scope}", scope);
            }
        }
        else
        {
            _logger.LogError("❌ Scopes are NULL or EMPTY! This will cause 'Missing required parameter: scope' error");
        }

        // Validate required settings
        if (string.IsNullOrEmpty(_settings.ClientId))
        {
            throw new InvalidOperationException("GoogleOAuth:ClientId is not configured");
        }
        
        if (string.IsNullOrEmpty(_settings.RedirectUri))
        {
            throw new InvalidOperationException("GoogleOAuth:RedirectUri is not configured");
        }
        
        if (_settings.Scopes == null || !_settings.Scopes.Any())
        {
            throw new InvalidOperationException("GoogleOAuth:Scopes is not configured or empty. At minimum, you need: openid, https://www.googleapis.com/auth/userinfo.email, https://www.googleapis.com/auth/userinfo.profile");
        }

        var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new ClientSecrets
            {
                ClientId = _settings.ClientId,
                ClientSecret = _settings.ClientSecret
            },
            Scopes = _settings.Scopes
        });

        var codeRequestUrl = flow.CreateAuthorizationCodeRequest(_settings.RedirectUri);
        codeRequestUrl.State = state;
        
        var authUrl = codeRequestUrl.Build().ToString();
        
        _logger.LogInformation("🔗 Generated base auth URL (length: {Length})", authUrl.Length);
        
        // Agregar parámetros para obtener refresh token
        if (!authUrl.Contains("access_type="))
        {
            var separator = authUrl.Contains("?") ? "&" : "?";
            authUrl += $"{separator}access_type=offline";
        }
        
        if (!authUrl.Contains("prompt="))
        {
            authUrl += "&prompt=consent";
        }
        
        _logger.LogInformation("✅ Final auth URL with offline access and consent prompt");
        _logger.LogInformation("🔗 Auth URL: {AuthUrl}", authUrl);
        
        return authUrl;
    }

    /// <summary>
    /// Intercambia el código de autorización por tokens de acceso
    /// </summary>
    public async Task<GoogleTokenResponse> ExchangeCodeForTokenAsync(string code)
    {
        var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new ClientSecrets
            {
                ClientId = _settings.ClientId,
                ClientSecret = _settings.ClientSecret
            },
            Scopes = _settings.Scopes,
            IncludeGrantedScopes = true
        });

        var token = await flow.ExchangeCodeForTokenAsync(
            "user",
            code,
            _settings.RedirectUri,
            CancellationToken.None
        );

        _logger.LogInformation("Token exchange successful. Has IdToken: {HasIdToken}, Has RefreshToken: {HasRefreshToken}", 
            !string.IsNullOrEmpty(token.IdToken),
            !string.IsNullOrEmpty(token.RefreshToken));

        return new GoogleTokenResponse
        {
            AccessToken = token.AccessToken,
            IdToken = token.IdToken,
            RefreshToken = token.RefreshToken,
            ExpiresInSeconds = token.ExpiresInSeconds ?? 3600
        };
    }
}
