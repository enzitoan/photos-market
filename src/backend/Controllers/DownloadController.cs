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
    private readonly ILogger<DownloadController> _logger;

    public DownloadController(
        IDownloadLinkRepository downloadLinkRepository,
        ILogger<DownloadController> logger)
    {
        _downloadLinkRepository = downloadLinkRepository;
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

            // Increment download count
            downloadLink.DownloadCount++;
            await _downloadLinkRepository.UpdateAsync(downloadLink);

            var downloadLinkDto = new DownloadLinkDto
            {
                Token = downloadLink.Token,
                DownloadUrl = $"/api/download/{token}/files",
                ExpiresAt = downloadLink.ExpiresAt,
                IsExpired = downloadLink.IsExpired
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
}
