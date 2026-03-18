using PhotosMarket.API.Models;

namespace PhotosMarket.API.Repositories;

public interface IPhotographerSettingsRepository
{
    Task<PhotographerSettings?> GetSettingsAsync();
    Task<PhotographerSettings> UpdateSettingsAsync(PhotographerSettings settings);
}
