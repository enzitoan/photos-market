using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhotosMarket.API.Services;
using PhotosMarket.API.Repositories;
using PhotosMarket.API.Models;
using PhotosMarket.API.DTOs;
using System.Security.Claims;

namespace PhotosMarket.API.Controllers;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize]
public class AlbumsController : ControllerBase
{
    private readonly GoogleDriveService _googleDriveService;
    private readonly IAlbumRepository _albumRepository;
    private readonly IPhotographerSettingsRepository _photographerSettingsRepository;
    private readonly ILogger<AlbumsController> _logger;

    public AlbumsController(
        GoogleDriveService googleDriveService,
        IAlbumRepository albumRepository,
        IPhotographerSettingsRepository photographerSettingsRepository,
        ILogger<AlbumsController> logger)
    {
        _googleDriveService = googleDriveService;
        _albumRepository = albumRepository;
        _photographerSettingsRepository = photographerSettingsRepository;
        _logger = logger;
    }

    private bool IsUserAdmin()
    {
        var roleClaimValue = User.FindFirst(ClaimTypes.Role)?.Value;
        return roleClaimValue == UserRole.Admin.ToString() || roleClaimValue == UserRole.Photographer.ToString();
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<AlbumManagementDto>>>> GetAllAlbums()
    {
        try
        {
            // Obtener álbumes de Google Drive (carpetas)
            var googleAlbums = await _googleDriveService.GetAlbumsAsync();

            // Obtener configuración de álbumes
            var albumConfigs = await _albumRepository.GetAllAsync();

            // Combinar información
            var result = googleAlbums.Select(ga =>
            {
                var config = albumConfigs.FirstOrDefault(ac => ac.GoogleAlbumId == ga.Id);
                return new AlbumManagementDto
                {
                    GoogleAlbumId = ga.Id,
                    Title = ga.Title,
                    CoverPhotoUrl = ga.CoverPhotoUrl,
                    MediaItemsCount = ga.MediaItemsCount,
                    IsBlocked = config?.IsBlocked ?? false,
                    DisplayOrder = config?.DisplayOrder ?? 0
                };
            }).ToList();

            return Ok(new ApiResponse<List<AlbumManagementDto>>
            {
                Success = true,
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching albums for management");
            return BadRequest(new ApiResponse<List<AlbumManagementDto>>
            {
                Success = false,
                Message = "Failed to fetch albums",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpPost("{googleAlbumId}/block")]
    public async Task<ActionResult<ApiResponse<bool>>> BlockAlbum(string googleAlbumId)
    {
        try
        {
            if (!IsUserAdmin())
                return Forbid();

            var album = await _albumRepository.GetByGoogleAlbumIdAsync(googleAlbumId);
            
            if (album == null)
            {
                album = new Album
                {
                    GoogleAlbumId = googleAlbumId,
                    IsBlocked = true
                };
                await _albumRepository.CreateAsync(album);
            }
            else
            {
                album.IsBlocked = true;
                await _albumRepository.UpdateAsync(album);
            }

            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Album blocked successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error blocking album {AlbumId}", googleAlbumId);
            return BadRequest(new ApiResponse<bool>
            {
                Success = false,
                Message = "Failed to block album",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpPost("{googleAlbumId}/unblock")]
    public async Task<ActionResult<ApiResponse<bool>>> UnblockAlbum(string googleAlbumId)
    {
        try
        {
            if (!IsUserAdmin())
                return Forbid();

            var album = await _albumRepository.GetByGoogleAlbumIdAsync(googleAlbumId);
            
            if (album == null)
            {
                album = new Album
                {
                    GoogleAlbumId = googleAlbumId,
                    IsBlocked = false
                };
                await _albumRepository.CreateAsync(album);
            }
            else
            {
                album.IsBlocked = false;
                await _albumRepository.UpdateAsync(album);
            }

            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Album unblocked successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unblocking album {AlbumId}", googleAlbumId);
            return BadRequest(new ApiResponse<bool>
            {
                Success = false,
                Message = "Failed to unblock album",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpGet("photographer-settings")]
    public async Task<ActionResult<ApiResponse<PhotographerSettingsDto>>> GetPhotographerSettings()
    {
        try
        {
            if (!IsUserAdmin())
                return Forbid();

            var settings = await _photographerSettingsRepository.GetSettingsAsync();
            
            if (settings == null)
            {
                settings = new PhotographerSettings();
                await _photographerSettingsRepository.UpdateSettingsAsync(settings);
            }

            var dto = new PhotographerSettingsDto
            {
                WatermarkText = settings.WatermarkText,
                WatermarkOpacity = settings.WatermarkOpacity,
                IsGoogleAuthenticated = !string.IsNullOrEmpty(settings.PhotographerGoogleRefreshToken)
            };

            return Ok(new ApiResponse<PhotographerSettingsDto>
            {
                Success = true,
                Data = dto
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching photographer settings");
            return BadRequest(new ApiResponse<PhotographerSettingsDto>
            {
                Success = false,
                Message = "Failed to fetch settings",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpPut("photographer-settings")]
    public async Task<ActionResult<ApiResponse<PhotographerSettingsDto>>> UpdatePhotographerSettings(
        [FromBody] UpdatePhotographerSettingsRequest request)
    {
        try
        {
            if (!IsUserAdmin())
                return Forbid();

            var settings = await _photographerSettingsRepository.GetSettingsAsync() ?? new PhotographerSettings();
            
            settings.WatermarkText = request.WatermarkText;
            settings.WatermarkOpacity = request.WatermarkOpacity;
            settings.PhotoPrice = request.PhotoPrice;
            
            await _photographerSettingsRepository.UpdateSettingsAsync(settings);

            var dto = new PhotographerSettingsDto
            {
                WatermarkText = settings.WatermarkText,
                WatermarkOpacity = settings.WatermarkOpacity,
                PhotoPrice = settings.PhotoPrice,
                IsGoogleAuthenticated = !string.IsNullOrEmpty(settings.PhotographerGoogleRefreshToken)
            };

            return Ok(new ApiResponse<PhotographerSettingsDto>
            {
                Success = true,
                Data = dto,
                Message = "Settings updated successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating photographer settings");
            return BadRequest(new ApiResponse<PhotographerSettingsDto>
            {
                Success = false,
                Message = "Failed to update settings",
                Errors = new List<string> { ex.Message }
            });
        }
    }
    
    [HttpPost("{googleAlbumId}/set-cover")]
    public async Task<ActionResult<ApiResponse<bool>>> SetCoverPhoto(string googleAlbumId, [FromBody] SetCoverPhotoRequest request)
    {
        try
        {
            if (!IsUserAdmin())
                return Forbid();

            var album = await _albumRepository.GetByGoogleAlbumIdAsync(googleAlbumId);
            
            if (album == null)
            {
                album = new Album
                {
                    GoogleAlbumId = googleAlbumId,
                    CoverPhotoId = request.PhotoId
                };
                await _albumRepository.CreateAsync(album);
            }
            else
            {
                album.CoverPhotoId = request.PhotoId;
                album.UpdatedAt = DateTime.UtcNow;
                await _albumRepository.UpdateAsync(album);
            }

            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Cover photo set successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cover photo for album {AlbumId}", googleAlbumId);
            return BadRequest(new ApiResponse<bool>
            {
                Success = false,
                Message = "Failed to set cover photo",
                Errors = new List<string> { ex.Message }
            });
        }
    }
}

// DTOs específicos para este controller
public class AlbumManagementDto
{
    public string GoogleAlbumId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? CoverPhotoUrl { get; set; }
    public int MediaItemsCount { get; set; }
    public bool IsBlocked { get; set; }
    public int DisplayOrder { get; set; }
}

public class PhotographerSettingsDto
{
    public string WatermarkText { get; set; } = string.Empty;
    public float WatermarkOpacity { get; set; }
    public decimal PhotoPrice { get; set; }
    public bool IsGoogleAuthenticated { get; set; }
}

public class UpdatePhotographerSettingsRequest
{
    public string WatermarkText { get; set; } = string.Empty;
    public float WatermarkOpacity { get; set; }
    public decimal PhotoPrice { get; set; }
}

public class SetCoverPhotoRequest
{
    public string PhotoId { get; set; } = string.Empty;
}
