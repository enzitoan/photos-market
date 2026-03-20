namespace PhotosMarket.API.Configuration;

public class ApplicationSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string FrontendUrl { get; set; } = string.Empty;
    public int DownloadLinkExpirationHours { get; set; } = 72;
    public string WatermarkText { get; set; } = string.Empty;
    public decimal PhotoPricePerUnit { get; set; } = 1000m;
    public string Currency { get; set; } = "CLP";
    
    // Discount Configuration
    public int BulkDiscountMinPhotos { get; set; } = 5;
    public decimal BulkDiscountPercentage { get; set; } = 20m;
}
