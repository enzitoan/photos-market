using PhotosMarket.API.Models;

namespace PhotosMarket.API.Repositories.InMemory;

public class InMemoryPhotographerSettingsRepository : IPhotographerSettingsRepository
{
    private PhotographerSettings _settings = new PhotographerSettings();

    public Task<PhotographerSettings?> GetSettingsAsync()
    {
        return Task.FromResult<PhotographerSettings?>(_settings);
    }

    public Task<PhotographerSettings> UpdateSettingsAsync(PhotographerSettings settings)
    {
        settings.UpdatedAt = DateTime.UtcNow;
        _settings = settings;
        return Task.FromResult(_settings);
    }
}
