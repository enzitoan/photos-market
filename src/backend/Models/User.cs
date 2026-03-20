using Newtonsoft.Json;

namespace PhotosMarket.API.Models;

public enum UserRole
{
    Customer,
    Photographer,
    Admin
}

public class User
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonProperty("googleUserId")]
    public string GoogleUserId { get; set; } = string.Empty;

    [JsonProperty("email")]
    public string Email { get; set; } = string.Empty;

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("role")]
    public UserRole Role { get; set; } = UserRole.Customer;

    [JsonProperty("googleRefreshToken")]
    public string? GoogleRefreshToken { get; set; }

    [JsonProperty("googleAccessToken")]
    public string? GoogleAccessToken { get; set; }

    [JsonProperty("googleTokenExpiresAt")]
    public DateTime? GoogleTokenExpiresAt { get; set; }

    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonProperty("lastLoginAt")]
    public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;

    [JsonProperty("phone")]
    public string? Phone { get; set; }

    [JsonProperty("idType")]
    public string? IdType { get; set; } // "RUT" o "DNI"

    [JsonProperty("idNumber")]
    public string? IdNumber { get; set; } // RUT o DNI según el tipo

    [JsonProperty("birthDate")]
    public DateTime? BirthDate { get; set; }

    [JsonProperty("isRegistrationComplete")]
    public bool IsRegistrationComplete { get; set; } = false;

    public bool IsAdmin => Role == UserRole.Admin || Role == UserRole.Photographer;
}
