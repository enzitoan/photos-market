using Microsoft.Azure.Cosmos;
using PhotosMarket.API.Models;
using PhotosMarket.API.Configuration;
using PhotosMarket.API.Services;
using Microsoft.Extensions.Options;

namespace PhotosMarket.API.Repositories;

public interface IDownloadLinkRepository
{
    Task<DownloadLink> CreateAsync(DownloadLink downloadLink);
    Task<DownloadLink?> GetByTokenAsync(string token);
    Task<DownloadLink?> GetByOrderIdAsync(string orderId, string userId);
    Task<DownloadLink> UpdateAsync(DownloadLink downloadLink);
}

public class DownloadLinkRepository : IDownloadLinkRepository
{
    private readonly Container _container;

    public DownloadLinkRepository(ICosmosDbService cosmosDbService, IOptions<CosmosDbSettings> settings)
    {
        _container = cosmosDbService.GetContainer(settings.Value.ContainerNames.DownloadLinks);
    }

    public async Task<DownloadLink> CreateAsync(DownloadLink downloadLink)
    {
        var response = await _container.CreateItemAsync(downloadLink, new PartitionKey(downloadLink.UserId));
        return response.Resource;
    }

    public async Task<DownloadLink?> GetByTokenAsync(string token)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.token = @token")
            .WithParameter("@token", token);

        var iterator = _container.GetItemQueryIterator<DownloadLink>(query);
        
        if (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            return response.FirstOrDefault();
        }

        return null;
    }

    public async Task<DownloadLink?> GetByOrderIdAsync(string orderId, string userId)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.orderId = @orderId AND c.userId = @userId")
            .WithParameter("@orderId", orderId)
            .WithParameter("@userId", userId);

        var iterator = _container.GetItemQueryIterator<DownloadLink>(query);
        
        if (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            return response.FirstOrDefault();
        }

        return null;
    }

    public async Task<DownloadLink> UpdateAsync(DownloadLink downloadLink)
    {
        var response = await _container.ReplaceItemAsync(downloadLink, downloadLink.Id, new PartitionKey(downloadLink.UserId));
        return response.Resource;
    }
}
