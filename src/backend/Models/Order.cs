using Newtonsoft.Json;

namespace PhotosMarket.API.Models;

public class Order
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonProperty("userId")]
    public string UserId { get; set; } = string.Empty;

    [JsonProperty("userEmail")]
    public string UserEmail { get; set; } = string.Empty;

    [JsonProperty("photos")]
    public List<OrderPhoto> Photos { get; set; } = new();

    [JsonProperty("subtotal")]
    public decimal Subtotal { get; set; }

    [JsonProperty("discountPercentage")]
    public decimal? DiscountPercentage { get; set; }

    [JsonProperty("discountAmount")]
    public decimal DiscountAmount { get; set; }

    [JsonProperty("totalAmount")]
    public decimal TotalAmount { get; set; }

    [JsonProperty("currency")]
    public string Currency { get; set; } = "USD";

    [JsonProperty("status")]
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonProperty("paidAt")]
    public DateTime? PaidAt { get; set; }

    [JsonProperty("processedAt")]
    public DateTime? ProcessedAt { get; set; }

    [JsonProperty("paymentReference")]
    public string? PaymentReference { get; set; }
}

public class OrderPhoto
{
    [JsonProperty("photoId")]
    public string PhotoId { get; set; } = string.Empty;

    [JsonProperty("mediaItemId")]
    public string MediaItemId { get; set; } = string.Empty;

    [JsonProperty("filename")]
    public string Filename { get; set; } = string.Empty;

    [JsonProperty("baseUrl")]
    public string BaseUrl { get; set; } = string.Empty;

    [JsonProperty("price")]
    public decimal Price { get; set; }

    [JsonProperty("albumId")]
    public string? AlbumId { get; set; }

    [JsonProperty("albumTitle")]
    public string? AlbumTitle { get; set; }
}

public enum OrderStatus
{
    Pending,
    AwaitingPayment,
    PaymentConfirmed,
    Processing,
    Completed,
    Cancelled
}
