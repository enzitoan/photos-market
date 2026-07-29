using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using PhotosMarket.API.Configuration;
using PhotosMarket.API.Models;
using PhotosMarket.API.Services;

namespace PhotosMarket.API.Repositories;

public interface IAlbumRepository
{
    Task<Album?> GetByGoogleAlbumIdAsync(string googleAlbumId);
    Task<List<Album>> GetAllAsync();
    Task<List<Album>> GetVisibleAlbumsAsync();
    Task<Album> CreateAsync(Album album);
    Task<Album> UpdateAsync(Album album);
    Task DeleteAsync(string id);
}

public class AlbumRepository : IAlbumRepository
{
    private readonly Container _container;

    public AlbumRepository(ICosmosDbService cosmosDbService, IOptions<CosmosDbSettings> settings)
    {
        _container = cosmosDbService.GetContainer(settings.Value.ContainerNames.Albums);
    }

    public async Task<Album?> GetByGoogleAlbumIdAsync(string googleAlbumId)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.googleAlbumId = @googleAlbumId")
            .WithParameter("@googleAlbumId", googleAlbumId);

        var iterator = _container.GetItemQueryIterator<Album>(query);
        var results = new List<Album>();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            results.AddRange(response);
        }

        return results.FirstOrDefault();
    }

    public async Task<List<Album>> GetAllAsync()
    {
        var query = new QueryDefinition("SELECT * FROM c");
        var iterator = _container.GetItemQueryIterator<Album>(query);
        var results = new List<Album>();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            results.AddRange(response);
        }

        return results.OrderBy(a => a.DisplayOrder).ToList();
    }

    public async Task<List<Album>> GetVisibleAlbumsAsync()
    {
        var albums = await GetAllAsync();
        return albums
            .Where(a => a.Visibility != AlbumVisibility.Blocked)
            .OrderBy(a => a.DisplayOrder)
            .ToList();
    }

    public async Task<Album> CreateAsync(Album album)
    {
        album.Id = string.IsNullOrWhiteSpace(album.Id) ? Guid.NewGuid().ToString() : album.Id;
        album.CreatedAt = album.CreatedAt == default ? DateTime.UtcNow : album.CreatedAt;
        album.UpdatedAt = DateTime.UtcNow;

        var response = await _container.CreateItemAsync(album, new PartitionKey(album.GoogleAlbumId));
        return response.Resource;
    }

    public async Task<Album> UpdateAsync(Album album)
    {
        album.UpdatedAt = DateTime.UtcNow;

        var response = await _container.UpsertItemAsync(album, new PartitionKey(album.GoogleAlbumId));
        return response.Resource;
    }

    public async Task DeleteAsync(string id)
    {
        var album = await GetByIdAsync(id);
        if (album is null)
        {
            return;
        }

        await _container.DeleteItemAsync<Album>(id, new PartitionKey(album.GoogleAlbumId));
    }

    private async Task<Album?> GetByIdAsync(string id)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.id = @id")
            .WithParameter("@id", id);

        var iterator = _container.GetItemQueryIterator<Album>(query);
        var results = new List<Album>();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            results.AddRange(response);
        }

        return results.FirstOrDefault();
    }
}
