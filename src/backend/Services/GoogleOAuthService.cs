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
        
        _logger.LogInformation("Generated auth URL with offline access and consent prompt");
        
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
