using PhotosMarket.API.Models;
using System.Collections.Concurrent;

namespace PhotosMarket.API.Repositories.InMemory;

public class InMemoryAlbumRepository : IAlbumRepository
{
    private readonly ConcurrentDictionary<string, Album> _albums = new();

    public Task<Album?> GetByGoogleAlbumIdAsync(string googleAlbumId)
    {
        var album = _albums.Values.FirstOrDefault(a => a.GoogleAlbumId == googleAlbumId);
        return Task.FromResult(album);
    }

    public Task<List<Album>> GetAllAsync()
    {
        return Task.FromResult(_albums.Values.OrderBy(a => a.DisplayOrder).ToList());
    }

    public Task<List<Album>> GetVisibleAlbumsAsync()
    {
        return Task.FromResult(_albums.Values
            .Where(a => !a.IsBlocked)
            .OrderBy(a => a.DisplayOrder)
            .ToList());
    }

    public Task<Album> CreateAsync(Album album)
    {
        album.Id = Guid.NewGuid().ToString();
        album.CreatedAt = DateTime.UtcNow;
        album.UpdatedAt = DateTime.UtcNow;
        _albums[album.Id] = album;
        return Task.FromResult(album);
    }

    public Task<Album> UpdateAsync(Album album)
    {
        album.UpdatedAt = DateTime.UtcNow;
        _albums[album.Id] = album;
        return Task.FromResult(album);
    }

    public Task DeleteAsync(string id)
    {
        _albums.TryRemove(id, out _);
        return Task.CompletedTask;
    }
}
