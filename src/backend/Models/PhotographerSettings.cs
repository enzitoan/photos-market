using Newtonsoft.Json;

namespace PhotosMarket.API.Models;

public class PhotographerSettings
{
    [JsonProperty("id")]
    public string Id { get; set; } = "photographer-settings";

    [JsonProperty("photographerGoogleEmail")]
    public string PhotographerGoogleEmail { get; set; } = "ahumada.enzo@gmail.com";

    [JsonProperty("photographerGoogleRefreshToken")]
    public string? PhotographerGoogleRefreshToken { get; set; }

    [JsonProperty("photographerGoogleAccessToken")]
    public string? PhotographerGoogleAccessToken { get; set; }

    [JsonProperty("photographerTokenExpiresAt")]
    public DateTime? PhotographerTokenExpiresAt { get; set; }

    [JsonProperty("watermarkText")]
    public string WatermarkText { get; set; } = "PhotosMarket © {YEAR}";

    [JsonProperty("watermarkOpacity")]
    public float WatermarkOpacity { get; set; } = 0.5f;

    [JsonProperty("photoPrice")]
    public decimal PhotoPrice { get; set; } = 1000.00m;

    [JsonProperty("currency")]
    public string Currency { get; set; } = "CLP";

    [JsonProperty("bulkDiscountMinPhotos")]
    public int? BulkDiscountMinPhotos { get; set; }

    [JsonProperty("bulkDiscountPercentage")]
    public decimal? BulkDiscountPercentage { get; set; }

    [JsonProperty("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
