using PhotosMarket.API.Models;

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
