using Microsoft.AspNetCore.Mvc;
using PhotosMarket.API.Services;
using PhotosMarket.API.DTOs;

namespace PhotosMarket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IGoogleOAuthService _googleOAuthService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        IGoogleOAuthService googleOAuthService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _googleOAuthService = googleOAuthService;
        _logger = logger;
    }

    [HttpGet("google-login")]
    public IActionResult GoogleLogin()
    {
        var state = Guid.NewGuid().ToString("N");
        var authUrl = _googleOAuthService.GetAuthorizationUrl(state);
        
        return Ok(new { authUrl });
    }

    [HttpPost("google-callback")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> GoogleCallback([FromBody] GoogleAuthCallbackRequest request)
    {
        try
        {
            _logger.LogInformation("Processing Google callback with code: {CodeLength} chars", request.Code?.Length ?? 0);
            
            // Validar que el código no sea nulo o vacío
            if (string.IsNullOrEmpty(request.Code))
            {
                return BadRequest(new ApiResponse<AuthResponse>
                {
                    Success = false,
                    Message = "Authorization code is missing",
                    Errors = new List<string> { "Code is required" }
                });
            }
            
            // Exchange code for tokens
            var tokenResponse = await _googleOAuthService.ExchangeCodeForTokenAsync(request.Code);
            
            _logger.LogInformation("Token exchange successful. IdToken present: {HasIdToken}", !string.IsNullOrEmpty(tokenResponse.IdToken));

            if (string.IsNullOrEmpty(tokenResponse.IdToken))
            {
                _logger.LogError("IdToken is null or empty");
                return BadRequest(new ApiResponse<AuthResponse>
                {
                    Success = false,
                    Message = "Failed to obtain ID token from Google",
                    Errors = new List<string> { "ID token is missing in the response" }
                });
            }

            // Authenticate user (cliente normal)
            var authResponse = await _authService.AuthenticateWithGoogleAsync(
                tokenResponse.IdToken, 
                tokenResponse.RefreshToken,
                tokenResponse.AccessToken);

            return Ok(new ApiResponse<AuthResponse>
            {
                Success = true,
                Data = authResponse
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Google authentication");
            return BadRequest(new ApiResponse<AuthResponse>
            {
                Success = false,
                Message = "Authentication failed",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpPost("photographer-google-callback")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> PhotographerGoogleCallback([FromBody] GoogleAuthCallbackRequest request)
    {
        try
        {
            _logger.LogInformation("Processing photographer Google callback with code: {CodeLength} chars", request.Code?.Length ?? 0);
            
            // Validar que el código no sea nulo o vacío
            if (string.IsNullOrEmpty(request.Code))
            {
                return BadRequest(new ApiResponse<AuthResponse>
                {
                    Success = false,
                    Message = "Authorization code is missing",
                    Errors = new List<string> { "Code is required" }
                });
            }
            
            // Exchange code for tokens
            var tokenResponse = await _googleOAuthService.ExchangeCodeForTokenAsync(request.Code);
            
            _logger.LogInformation("Token exchange successful. IdToken present: {HasIdToken}, RefreshToken present: {HasRefreshToken}, AccessToken present: {HasAccessToken}", 
                !string.IsNullOrEmpty(tokenResponse.IdToken),
                !string.IsNullOrEmpty(tokenResponse.RefreshToken),
                !string.IsNullOrEmpty(tokenResponse.AccessToken));

            if (string.IsNullOrEmpty(tokenResponse.IdToken))
            {
                _logger.LogError("IdToken is null or empty");
                return BadRequest(new ApiResponse<AuthResponse>
                {
                    Success = false,
                    Message = "Failed to obtain ID token from Google",
                    Errors = new List<string> { "ID token is missing in the response" }
                });
            }

            if (string.IsNullOrEmpty(tokenResponse.RefreshToken))
            {
                _logger.LogWarning("RefreshToken is null or empty - Google Photos access may not persist");
            }

            // Authenticate photographer (guarda tokens en PhotographerSettings)
            var authResponse = await _authService.AuthenticatePhotographerWithGoogleAsync(
                tokenResponse.IdToken, 
                tokenResponse.RefreshToken,
                tokenResponse.AccessToken);

            _logger.LogInformation("Photographer authenticated successfully. Email: {Email}", authResponse.Email);

            return Ok(new ApiResponse<AuthResponse>
            {
                Success = true,
                Data = authResponse
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Unauthorized photographer authentication attempt");
            return Unauthorized(new ApiResponse<AuthResponse>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during photographer Google authentication");
            return BadRequest(new ApiResponse<AuthResponse>
            {
                Success = false,
                Message = "Authentication failed",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpGet("validate")]
    public async Task<IActionResult> ValidateToken()
    {
        var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        
        if (string.IsNullOrEmpty(token))
            return Unauthorized();

        var user = await _authService.GetUserFromTokenAsync(token);
        
        if (user == null)
            return Unauthorized();

        return Ok(new { userId = user.Id, email = user.Email, name = user.Name });
    }

    [HttpPost("admin-login")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> AdminLogin([FromBody] AdminLoginRequest request)
    {
        try
        {
            // Validar credenciales simples
            if (request.Username != "admin" || request.Password != "admin")
            {
                return Unauthorized(new ApiResponse<AuthResponse>
                {
                    Success = false,
                    Message = "Credenciales inválidas"
                });
            }

            var authResponse = await _authService.AuthenticateAdminAsync(request.Username);

            return Ok(new ApiResponse<AuthResponse>
            {
                Success = true,
                Data = authResponse
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during admin authentication");
            return BadRequest(new ApiResponse<AuthResponse>
            {
                Success = false,
                Message = "Authentication failed",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpPost("complete-registration")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> CompleteRegistration([FromBody] CompleteRegistrationRequest request)
    {
        try
        {
            // Obtener userId del token
            var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized(new ApiResponse<AuthResponse>
                {
                    Success = false,
                    Message = "Token no proporcionado"
                });
            }

            var user = await _authService.GetUserFromTokenAsync(token);
            
            if (user == null)
            {
                return Unauthorized(new ApiResponse<AuthResponse>
                {
                    Success = false,
                    Message = "Token inválido"
                });
            }

            // Completar registro
            var authResponse = await _authService.CompleteRegistrationAsync(user.Id, request);

            _logger.LogInformation("User {UserId} completed registration successfully", user.Id);

            return Ok(new ApiResponse<AuthResponse>
            {
                Success = true,
                Data = authResponse,
                Message = "Registro completado exitosamente"
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error during registration completion");
            return BadRequest(new ApiResponse<AuthResponse>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation during registration completion");
            return BadRequest(new ApiResponse<AuthResponse>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration completion");
            return BadRequest(new ApiResponse<AuthResponse>
            {
                Success = false,
                Message = "Error al completar el registro",
                Errors = new List<string> { ex.Message }
            });
        }
    }
}
