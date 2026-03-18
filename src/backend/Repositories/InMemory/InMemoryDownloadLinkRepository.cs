using PhotosMarket.API.Models;
using System.Collections.Concurrent;

namespace PhotosMarket.API.Repositories.InMemory;

public class InMemoryDownloadLinkRepository : IDownloadLinkRepository
{
    private readonly ConcurrentDictionary<string, DownloadLink> _links = new();

    public Task<DownloadLink> CreateAsync(DownloadLink downloadLink)
    {
        _links[downloadLink.Id] = downloadLink;
        return Task.FromResult(downloadLink);
    }

    public Task<DownloadLink?> GetByTokenAsync(string token)
    {
        var link = _links.Values.FirstOrDefault(l => l.Token == token);
        return Task.FromResult(link);
    }

    public Task<DownloadLink?> GetByOrderIdAsync(string orderId, string userId)
    {
        var link = _links.Values.FirstOrDefault(l => l.OrderId == orderId && l.UserId == userId);
        return Task.FromResult(link);
    }

    public Task<DownloadLink> UpdateAsync(DownloadLink downloadLink)
    {
        _links[downloadLink.Id] = downloadLink;
        return Task.FromResult(downloadLink);
    }
}
