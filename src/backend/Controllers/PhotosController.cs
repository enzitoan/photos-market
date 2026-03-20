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
    public async Task<ActionResult<ApiResponse<List<AlbumDto>>>> GetAlbums()
    {
        try
        {
            var albums = await _googleDriveService.GetAlbumsAsync();

            // Obtener álbumes bloqueados
            var blockedAlbums = await _albumRepository.GetAllAsync();
            var blockedIds = blockedAlbums.Where(a => a.IsBlocked).Select(a => a.GoogleAlbumId).ToHashSet();

            // Filtrar álbumes bloqueados para usuarios normales
            var filteredAlbums = albums.Where(a => !blockedIds.Contains(a.Id)).ToList();

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
    public async Task<ActionResult<ApiResponse<AlbumDto>>> GetAlbum(string albumId)
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

            // Verificar si está bloqueado
            var albumConfig = await _albumRepository.GetByGoogleAlbumIdAsync(albumId);
            if (albumConfig?.IsBlocked == true)
            {
                return Forbid();
            }

            return Ok(new ApiResponse<AlbumDto>
            {
                Success = true,
                Data = album
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
    public async Task<ActionResult<ApiResponse<List<PhotoDto>>>> GetAlbumPhotos(string albumId)
    {
        try
        {
            // Primero obtener información del álbum
            var album = await _googleDriveService.GetAlbumByIdAsync(albumId);
            
            var photos = await _googleDriveService.GetPhotosFromAlbumAsync(albumId);

            // Agregar información del álbum a cada foto
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
