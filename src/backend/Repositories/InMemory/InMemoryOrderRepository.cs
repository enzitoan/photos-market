using PhotosMarket.API.Models;
using System.Collections.Concurrent;

namespace PhotosMarket.API.Repositories.InMemory;

public class InMemoryOrderRepository : IOrderRepository
{
    private readonly ConcurrentDictionary<string, Order> _orders = new();

    public Task<Order> CreateAsync(Order order)
    {
        _orders[order.Id] = order;
        return Task.FromResult(order);
    }

    public Task<Order?> GetByIdAsync(string id, string userId)
    {
        if (_orders.TryGetValue(id, out var order) && order.UserId == userId)
        {
            return Task.FromResult<Order?>(order);
        }
        return Task.FromResult<Order?>(null);
    }

    public Task<Order?> GetByIdAsync(string id)
    {
        if (_orders.TryGetValue(id, out var order))
        {
            return Task.FromResult<Order?>(order);
        }
        return Task.FromResult<Order?>(null);
    }

    public Task<List<Order>> GetByUserIdAsync(string userId)
    {
        var orders = _orders.Values
            .Where(o => o.UserId == userId)
            .ToList();
        return Task.FromResult(orders);
    }

    public Task<List<Order>> GetAllAsync()
    {
        var orders = _orders.Values
            .OrderByDescending(o => o.CreatedAt)
            .ToList();
        return Task.FromResult(orders);
    }

    public Task<Order> UpdateAsync(Order order)
    {
        _orders[order.Id] = order;
        return Task.FromResult(order);
    }

    public Task DeleteAsync(string id, string userId)
    {
        if (_orders.TryGetValue(id, out var order) && order.UserId == userId)
        {
            _orders.TryRemove(id, out _);
        }
        return Task.CompletedTask;
    }
}
