using Microsoft.Azure.Cosmos;
using PhotosMarket.API.Configuration;
using PhotosMarket.API.Services;
using Microsoft.Extensions.Options;
using PhotosMarket.API.Models;

namespace PhotosMarket.API.Repositories;

public class PhotographerSettingsCosmosRepository : IPhotographerSettingsRepository
{
    private readonly Container _container;
    private const string SETTINGS_ID = "photographer-settings";

    public PhotographerSettingsCosmosRepository(ICosmosDbService cosmosDbService, IOptions<CosmosDbSettings> settings)
    {
        _container = cosmosDbService.GetContainer(settings.Value.ContainerNames.PhotographerSettings);
    }

    public async Task<PhotographerSettings?> GetSettingsAsync()
    {
        try
        {
            var response = await _container.ReadItemAsync<PhotographerSettings>(
                SETTINGS_ID, 
                new PartitionKey(SETTINGS_ID));
            
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // Settings don't exist yet, return null
            return null;
        }
    }

    public async Task<PhotographerSettings> UpdateSettingsAsync(PhotographerSettings settings)
    {
        settings.Id = SETTINGS_ID; // Ensure ID is set
        settings.UpdatedAt = DateTime.UtcNow;
        
        try
        {
            // Try to replace if exists
            var response = await _container.ReplaceItemAsync(
                settings, 
                SETTINGS_ID, 
                new PartitionKey(SETTINGS_ID));
            
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // Settings don't exist, create them
            var response = await _container.CreateItemAsync(
                settings, 
                new PartitionKey(SETTINGS_ID));
            
            return response.Resource;
        }
    }
}
