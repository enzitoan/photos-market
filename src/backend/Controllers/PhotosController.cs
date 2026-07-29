using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PhotosMarket.API.Configuration;
using PhotosMarket.API.Services;
using PhotosMarket.API.Repositories;
using PhotosMarket.API.DTOs;
using System.Security.Claims;

namespace PhotosMarket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PhotosController : ControllerBase
{
    private readonly GoogleDriveService _googleDriveService;
    private readonly IAlbumRepository _albumRepository;
    private readonly IPhotographerSettingsRepository _photographerSettingsRepository;
    private readonly ApplicationSettings _appSettings;
    private readonly ILogger<PhotosController> _logger;

    public PhotosController(
        GoogleDriveService googleDriveService,
        IAlbumRepository albumRepository,
        IPhotographerSettingsRepository photographerSettingsRepository,
        IOptions<ApplicationSettings> appSettings,
        ILogger<PhotosController> logger)
    {
        _googleDriveService = googleDriveService;
        _albumRepository = albumRepository;
        _photographerSettingsRepository = photographerSettingsRepository;
        _appSettings = appSettings.Value;
        _logger = logger;
    }

    [HttpGet("config")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<PublicConfigDto>>> GetPublicConfig()
    {
        try
        {
            var settings = await _photographerSettingsRepository.GetSettingsAsync();
            
            if (settings == null)
            {
                settings = new Models.PhotographerSettings();
            }

            var config = new PublicConfigDto
            {
                WatermarkText = settings.WatermarkText,
                WatermarkOpacity = settings.WatermarkOpacity,
                PhotoPrice = settings.PhotoPrice,
                Currency = settings.Currency,
                BulkDiscountMinPhotos = settings.BulkDiscountMinPhotos ?? _appSettings.BulkDiscountMinPhotos,
                BulkDiscountPercentage = settings.BulkDiscountPercentage ?? _appSettings.BulkDiscountPercentage
            };

            return Ok(new ApiResponse<PublicConfigDto>
            {
                Success = true,
                Data = config
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching public config");
            return BadRequest(new ApiResponse<PublicConfigDto>
            {
                Success = false,
                Message = "Failed to fetch config",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpGet("albums")]
    public async Task<ActionResult<ApiResponse<List<AlbumDto>>>> GetAlbums([FromQuery] string? accessCode)
    {
        try
        {
            var albums = await _googleDriveService.GetAlbumsAsync();
            var albumConfigs = await _albumRepository.GetAllAsync();
            var now = DateTime.UtcNow;

            var filteredAlbums = albums.Select(ga =>
            {
                var config = albumConfigs.FirstOrDefault(ac => ac.GoogleAlbumId == ga.Id);
                var isVisible = IsAlbumVisible(config, accessCode, now);
                return new { Album = ga, Config = config, IsVisible = isVisible };
            })
            .Where(x => x.IsVisible)
            .Select(x => new AlbumDto
            {
                Id = x.Album.Id,
                Title = x.Album.Title,
                CoverPhotoUrl = x.Album.CoverPhotoUrl,
                MediaItemsCount = x.Album.MediaItemsCount,
                IsBlocked = x.Config?.IsBlocked ?? false,
                Visibility = x.Config?.Visibility ?? Models.AlbumVisibility.Public,
                HasAccessCode = !string.IsNullOrWhiteSpace(x.Config?.AccessCodeHash)
            })
            .ToList();

            return Ok(new ApiResponse<List<AlbumDto>>
            {
                Success = true,
                Data = filteredAlbums
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching albums");
            return BadRequest(new ApiResponse<List<AlbumDto>>
            {
                Success = false,
                Message = "Failed to fetch albums",
                Errors = new List<string> { ex.Message }
            });
        }
    }
    
    [HttpGet("albums/{albumId}")]
    public async Task<ActionResult<ApiResponse<AlbumDto>>> GetAlbum(string albumId, [FromQuery] string? accessCode)
    {
        try
        {
            var album = await _googleDriveService.GetAlbumByIdAsync(albumId);
            
            if (album == null)
            {
                return NotFound(new ApiResponse<AlbumDto>
                {
                    Success = false,
                    Message = "Album not found"
                });
            }

            var albumConfig = await _albumRepository.GetByGoogleAlbumIdAsync(albumId);
            if (!IsAlbumVisible(albumConfig, accessCode, DateTime.UtcNow))
            {
                return Forbid();
            }

            return Ok(new ApiResponse<AlbumDto>
            {
                Success = true,
                Data = new AlbumDto
                {
                    Id = album.Id,
                    Title = album.Title,
                    CoverPhotoUrl = album.CoverPhotoUrl,
                    MediaItemsCount = album.MediaItemsCount,
                    IsBlocked = albumConfig?.IsBlocked ?? false,
                    Visibility = albumConfig?.Visibility ?? Models.AlbumVisibility.Public,
                    HasAccessCode = !string.IsNullOrWhiteSpace(albumConfig?.AccessCodeHash)
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching album {AlbumId}", albumId);
            return BadRequest(new ApiResponse<AlbumDto>
            {
                Success = false,
                Message = "Failed to fetch album",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpGet("albums/{albumId}/photos")]
    public async Task<ActionResult<ApiResponse<List<PhotoDto>>>> GetAlbumPhotos(string albumId, [FromQuery] string? accessCode)
    {
        try
        {
            var albumConfig = await _albumRepository.GetByGoogleAlbumIdAsync(albumId);
            if (!IsAlbumVisible(albumConfig, accessCode, DateTime.UtcNow))
            {
                return Forbid();
            }

            var album = await _googleDriveService.GetAlbumByIdAsync(albumId);
            var photos = await _googleDriveService.GetPhotosFromAlbumAsync(albumId);

            if (album != null)
            {
                foreach (var photo in photos)
                {
                    photo.AlbumId = albumId;
                    photo.AlbumTitle = album.Title;
                }
            }

            return Ok(new ApiResponse<List<PhotoDto>>
            {
                Success = true,
                Data = photos
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching photos from album {AlbumId}", albumId);
            return BadRequest(new ApiResponse<List<PhotoDto>>
            {
                Success = false,
                Message = "Failed to fetch photos",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    private static bool IsAlbumVisible(Models.Album? albumConfig, string? accessCode, DateTime now)
    {
        if (albumConfig == null)
        {
            return true;
        }

        if (albumConfig.IsBlocked || albumConfig.Visibility == Models.AlbumVisibility.Blocked)
        {
            return false;
        }

        if (albumConfig.Visibility == Models.AlbumVisibility.Public)
        {
            return true;
        }

        if (albumConfig.Visibility == Models.AlbumVisibility.Private)
        {
            if (string.IsNullOrWhiteSpace(accessCode) || string.IsNullOrWhiteSpace(albumConfig.AccessCodeHash))
            {
                return false;
            }

            if (albumConfig.AccessCodeExpiresAt.HasValue && albumConfig.AccessCodeExpiresAt.Value <= now)
            {
                return false;
            }

            return HashAccessCode(accessCode) == albumConfig.AccessCodeHash;
        }

        return true;
    }

    private static string HashAccessCode(string accessCode)
    {
        return Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(accessCode)));
    }

    /// <summary>
    /// Proxy endpoint para servir imágenes desde Google Drive
    /// Resuelve problemas de CORS y autenticación
    /// </summary>
    [HttpGet("proxy/{fileId}")]
    [AllowAnonymous]
    public async Task<IActionResult> ProxyImage(string fileId, [FromQuery] string? size = "medium")
    {
        try
        {
            _logger.LogInformation("Proxying image request for file {FileId}, size: {Size}", fileId, size);
            
            var stream = await _googleDriveService.DownloadPhotoAsync(fileId);
            
            // Obtener metadata para el tipo MIME
            var metadata = await _googleDriveService.GetPhotoMetadataAsync(fileId);
            var mimeType = metadata?.MimeType ?? "image/jpeg";
            
            return File(stream, mimeType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error proxying image {FileId}", fileId);
            return NotFound();
        }
    }

    // ⚠️ DEPRECATED: Este endpoint requiere actualización para Google Drive
    // Por ahora, usar el endpoint de descarga directa
    /*
    [HttpGet("{mediaItemId}")]
    public async Task<ActionResult<ApiResponse<PhotoDto>>> GetPhoto(string mediaItemId)
    {
        try
        {
            var metadata = await _googleDriveService.GetPhotoMetadataAsync(mediaItemId);

            if (metadata == null)
                return NotFound();

            var photo = new PhotoDto
            {
                Id = metadata.Id,
                Filename = metadata.Name,
                ThumbnailUrl = metadata.ThumbnailLink ?? "",
                OriginalUrl = metadata.WebContentLink ?? "",
                MimeType = metadata.MimeType,
                CreatedAt = metadata.CreatedTime ?? DateTime.UtcNow
            };

            return Ok(new ApiResponse<PhotoDto>
            {
                Success = true,
                Data = photo
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching photo {MediaItemId}", mediaItemId);
            return BadRequest(new ApiResponse<PhotoDto>
            {
                Success = false,
                Message = "Failed to fetch photo",
                Errors = new List<string> { ex.Message }
            });
        }
    }
    */
}
