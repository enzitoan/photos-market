using PhotosMarket.API.Models;
using System.Collections.Concurrent;

namespace PhotosMarket.API.Repositories.InMemory;

public class InMemoryUserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<string, User> _users = new();

    public Task<User> CreateAsync(User user)
    {
        _users[user.Id] = user;
        return Task.FromResult(user);
    }

    public Task<User?> GetByIdAsync(string id)
    {
        _users.TryGetValue(id, out var user);
        return Task.FromResult(user);
    }

    public Task<User?> GetByGoogleUserIdAsync(string googleUserId)
    {
        var user = _users.Values.FirstOrDefault(u => u.GoogleUserId == googleUserId);
        return Task.FromResult(user);
    }

    public Task<User?> GetByEmailAsync(string email)
    {
        var user = _users.Values.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(user);
    }

    public Task<User> UpdateAsync(User user)
    {
        _users[user.Id] = user;
        return Task.FromResult(user);
    }
}
