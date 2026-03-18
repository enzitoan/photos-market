using Newtonsoft.Json;

namespace PhotosMarket.API.Models;

public class Album
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonProperty("googleAlbumId")]
    public string GoogleAlbumId { get; set; } = string.Empty;

    [JsonProperty("title")]
    public string Title { get; set; } = string.Empty;
    
    [JsonProperty("coverPhotoId")]
    public string? CoverPhotoId { get; set; }

    [JsonProperty("isBlocked")]
    public bool IsBlocked { get; set; } = false;

    [JsonProperty("displayOrder")]
    public int DisplayOrder { get; set; } = 0;

    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonProperty("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
