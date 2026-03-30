using Microsoft.AspNetCore.Mvc;
using PhotosMarket.API.Services;
using PhotosMarket.API.Repositories;
using PhotosMarket.API.DTOs;

namespace PhotosMarket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DownloadController : ControllerBase
{
    private readonly IDownloadLinkRepository _downloadLinkRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly GoogleDriveService _googleDriveService;
    private readonly IWatermarkService _watermarkService;
    private readonly ILogger<DownloadController> _logger;

    public DownloadController(
        IDownloadLinkRepository downloadLinkRepository,
        IOrderRepository orderRepository,
        GoogleDriveService googleDriveService,
        IWatermarkService watermarkService,
        ILogger<DownloadController> logger)
    {
        _downloadLinkRepository = downloadLinkRepository;
        _orderRepository = orderRepository;
        _googleDriveService = googleDriveService;
        _watermarkService = watermarkService;
        _logger = logger;
    }

    [HttpGet("{token}")]
    public async Task<ActionResult<ApiResponse<DownloadLinkDto>>> GetDownloadLink(string token)
    {
        try
        {
            var downloadLink = await _downloadLinkRepository.GetByTokenAsync(token);

            if (downloadLink == null)
            {
                return NotFound(new ApiResponse<DownloadLinkDto>
                {
                    Success = false,
                    Message = "Download link not found"
                });
            }

            if (downloadLink.IsExpired)
            {
                return BadRequest(new ApiResponse<DownloadLinkDto>
                {
                    Success = false,
                    Message = "Download link has expired"
                });
            }

            // Get order information
            var allOrders = await _orderRepository.GetAllAsync();
            var order = allOrders.FirstOrDefault(o => o.Id == downloadLink.OrderId);

            if (order == null)
            {
                return NotFound(new ApiResponse<DownloadLinkDto>
                {
                    Success = false,
                    Message = "Order not found for this download link"
                });
            }

            // Increment download count
            downloadLink.DownloadCount++;
            await _downloadLinkRepository.UpdateAsync(downloadLink);

            var downloadLinkDto = new DownloadLinkDto
            {
                Token = downloadLink.Token,
                DownloadUrl = $"/api/download/{token}/files",
                ExpiresAt = downloadLink.ExpiresAt,
                IsExpired = downloadLink.IsExpired,
                OrderNumber = order.Id.Substring(0, 8).ToUpper(),
                CustomerName = order.UserName,
                CustomerEmail = order.UserEmail,
                PhotoIds = order.Photos.Select(p => p.PhotoId).ToList(),
                Photos = order.Photos.Select(p => new DownloadPhotoDto
                {
                    PhotoId = p.PhotoId,
                    Filename = p.Filename,
                    // Usar el mismo formato que PhotoCard pero sin watermark
                    // PhotoId es el fileId de Google Drive
                    ThumbnailUrl = $"https://lh3.googleusercontent.com/d/{p.PhotoId}=w400"
                }).ToList()
            };

            return Ok(new ApiResponse<DownloadLinkDto>
            {
                Success = true,
                Data = downloadLinkDto
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching download link for token {Token}", token);
            return BadRequest(new ApiResponse<DownloadLinkDto>
            {
                Success = false,
                Message = "Failed to fetch download link",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpGet("{token}/files")]
    public async Task<IActionResult> GetFiles(string token)
    {
        try
        {
            var downloadLink = await _downloadLinkRepository.GetByTokenAsync(token);

            if (downloadLink == null)
                return NotFound("Download link not found");

            if (downloadLink.IsExpired)
                return BadRequest("Download link has expired");

            // Return the list of photo URLs
            // In a real implementation, you might want to create a ZIP file
            // or stream the files directly
            return Ok(new ApiResponse<List<string>>
            {
                Success = true,
                Data = downloadLink.PhotoUrls,
                Message = $"Found {downloadLink.PhotoUrls.Count} photos available for download"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading files for token {Token}", token);
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{token}/photo/{photoId}")]
    public async Task<IActionResult> GetSinglePhoto(string token, string photoId)
    {
        try
        {
            var downloadLink = await _downloadLinkRepository.GetByTokenAsync(token);

            if (downloadLink == null)
                return NotFound(new { error = "Download link not found" });

            if (downloadLink.IsExpired)
                return BadRequest(new { error = "Download link has expired" });

            // Get order to match photoId with photo info
            var allOrders = await _orderRepository.GetAllAsync();
            var order = allOrders.FirstOrDefault(o => o.Id == downloadLink.OrderId);

            if (order == null)
                return NotFound(new { error = "Order not found" });

            // Find the photo by ID
            var photo = order.Photos.FirstOrDefault(p => p.PhotoId == photoId);
            
            if (photo == null)
                return NotFound(new { error = "Photo not found in this order" });

            _logger.LogInformation("Downloading photo {PhotoId} (filename: {Filename}) for order {OrderId}", photoId, photo.Filename, order.Id);

            // Download the photo directly from Google Drive using the service
            var photoStream = await _googleDriveService.DownloadPhotoAsync(photoId);

            // Aplicar marca de agua server-side solo si es una imagen
            var extension = Path.GetExtension(photo.Filename).ToLowerInvariant();
            var isImage = extension is ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" or ".bmp";
            
            Stream finalStream;
            if (isImage)
            {
                _logger.LogInformation("Aplicando marca de agua a {Filename}", photo.Filename);
                try
                {
                    finalStream = await _watermarkService.ApplyWatermarkAsync(photoStream);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error aplicando marca de agua a {Filename}, devolviendo imagen original", photo.Filename);
                    photoStream.Position = 0; // Reset stream position
                    finalStream = photoStream;
                }
            }
            else
            {
                _logger.LogInformation("Archivo {Filename} no es una imagen, devolviendo sin marca de agua", photo.Filename);
                finalStream = photoStream;
            }

            // Determine the content type based on the filename
            var contentType = GetContentType(photo.Filename);
            
            // Return the file directly to the client
            return File(finalStream, contentType, photo.Filename);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading single photo {PhotoId} for token {Token}", photoId, token);
            return BadRequest(new { error = ex.Message });
        }
    }

    private string GetContentType(string filename)
    {
        var extension = Path.GetExtension(filename).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".bmp" => "image/bmp",
            ".heic" => "image/heic",
            _ => "application/octet-stream"
        };
    }
}
