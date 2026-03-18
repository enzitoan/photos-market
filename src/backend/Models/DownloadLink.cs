using Newtonsoft.Json;

namespace PhotosMarket.API.Models;

public class DownloadLink
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonProperty("orderId")]
    public string OrderId { get; set; } = string.Empty;

    [JsonProperty("userId")]
    public string UserId { get; set; } = string.Empty;

    [JsonProperty("token")]
    public string Token { get; set; } = Guid.NewGuid().ToString("N");

    [JsonProperty("photoUrls")]
    public List<string> PhotoUrls { get; set; } = new();

    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonProperty("expiresAt")]
    public DateTime ExpiresAt { get; set; }

    [JsonProperty("isExpired")]
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;

    [JsonProperty("downloadCount")]
    public int DownloadCount { get; set; } = 0;
}
