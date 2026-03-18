using Microsoft.Extensions.Options;
using PhotosMarket.API.Configuration;

namespace PhotosMarket.API.Services;

public interface IWatermarkService
{
    string GetWatermarkText();
}

public class WatermarkService : IWatermarkService
{
    private readonly ApplicationSettings _settings;

    public WatermarkService(IOptions<ApplicationSettings> settings)
    {
        _settings = settings.Value;
    }

    public string GetWatermarkText()
    {
        return _settings.WatermarkText.Replace("{YEAR}", DateTime.Now.Year.ToString());
    }

    // NOTE: Watermark application should be handled on the client side (frontend)
    // or using a cross-platform image processing library like ImageSharp
    // System.Drawing.Common is Windows-only in .NET Core 3.0+
    //
    // For production, consider using:
    // - SixLabors.ImageSharp (cross-platform)
    // - Client-side watermarking with Canvas API
    // - Azure Computer Vision or similar cloud services
}
