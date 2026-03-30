namespace PhotosMarket.API.DTOs;

// Authentication DTOs
public class GoogleAuthCallbackRequest
{
    public string Code { get; set; } = string.Empty;
}

public class AdminLoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
    public bool NeedsRegistration { get; set; } = false;
    public string? TempToken { get; set; }
}

public class CompleteRegistrationRequest
{
    public string Phone { get; set; } = string.Empty;
    public string IdType { get; set; } = string.Empty; // "RUT" o "DNI"
    public string IdNumber { get; set; } = string.Empty; // RUT o DNI según el tipo
    public DateTime BirthDate { get; set; }
}

// Photo DTOs
public class AlbumDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? CoverPhotoUrl { get; set; }
    public int MediaItemsCount { get; set; }
}

public class PhotoDto
{
    public string Id { get; set; } = string.Empty;
    public string MediaItemId { get; set; } = string.Empty;
    public string Filename { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public DateTime? CreationTime { get; set; }
    public PhotoMetadataDto? Metadata { get; set; }
    public string? AlbumId { get; set; }
    public string? AlbumTitle { get; set; }
}

public class PhotoMetadataDto
{
    public string? CameraMake { get; set; }
    public string? CameraModel { get; set; }
    public DateTime? CreationTime { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}

// Shopping Cart DTOs
public class AddToCartRequest
{
    public List<string> PhotoIds { get; set; } = new();
}

public class CartItemDto
{
    public string PhotoId { get; set; } = string.Empty;
    public string MediaItemId { get; set; } = string.Empty;
    public string Filename { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? AlbumId { get; set; }
    public string? AlbumTitle { get; set; }
}

public class CartDto
{
    public List<CartItemDto> Items { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "USD";
}

// Order DTOs
public class CreateOrderRequest
{
    public string UserName { get; set; } = string.Empty;
    public List<OrderPhotoDto> Photos { get; set; } = new();
}

public class OrderPhotoDto
{
    public string PhotoId { get; set; } = string.Empty;
    public string MediaItemId { get; set; } = string.Empty;
    public string Filename { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string? AlbumId { get; set; }
    public string? AlbumTitle { get; set; }
}

public class OrderDto
{
    public string Id { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public List<OrderPhotoDto> Photos { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? PaymentReference { get; set; }
}

public class ConfirmPaymentRequest
{
    public string OrderId { get; set; } = string.Empty;
    public string PaymentReference { get; set; } = string.Empty;
}

// Download DTOs
public class DownloadLinkDto
{
    public string Token { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsExpired { get; set; }    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public List<string> PhotoIds { get; set; } = new();
    public List<DownloadPhotoDto> Photos { get; set; } = new();
}

public class DownloadPhotoDto
{
    public string PhotoId { get; set; } = string.Empty;
    public string Filename { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
}

// Public Configuration DTOs
public class PublicConfigDto
{
    public string WatermarkText { get; set; } = string.Empty;
    public float WatermarkOpacity { get; set; }
    public decimal PhotoPrice { get; set; }
    public string Currency { get; set; } = "CLP";
    public int BulkDiscountMinPhotos { get; set; }
    public decimal BulkDiscountPercentage { get; set; }
}

// Response wrapper
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }
}
