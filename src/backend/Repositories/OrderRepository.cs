using Microsoft.Azure.Cosmos;
using PhotosMarket.API.Models;
using PhotosMarket.API.Configuration;
using PhotosMarket.API.Services;
using Microsoft.Extensions.Options;

namespace PhotosMarket.API.Repositories;

public interface IOrderRepository
{
    Task<Order> CreateAsync(Order order);
    Task<Order?> GetByIdAsync(string id, string userId);
    Task<Order?> GetByIdAsync(string id);
    Task<List<Order>> GetByUserIdAsync(string userId);
    Task<List<Order>> GetAllAsync();
    Task<Order> UpdateAsync(Order order);
    Task DeleteAsync(string id, string userId);
}

public class OrderRepository : IOrderRepository
{
    private readonly Container _container;

    public OrderRepository(ICosmosDbService cosmosDbService, IOptions<CosmosDbSettings> settings)
    {
        _container = cosmosDbService.GetContainer(settings.Value.ContainerNames.Orders);
    }

    public async Task<Order> CreateAsync(Order order)
    {
        var response = await _container.CreateItemAsync(order, new PartitionKey(order.UserId));
        return response.Resource;
    }

    public async Task<Order?> GetByIdAsync(string id, string userId)
    {
        try
        {
            var response = await _container.ReadItemAsync<Order>(id, new PartitionKey(userId));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<Order?> GetByIdAsync(string id)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.id = @id")
            .WithParameter("@id", id);

        var iterator = _container.GetItemQueryIterator<Order>(query);
        
        if (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            return response.FirstOrDefault();
        }

        return null;
    }

    public async Task<List<Order>> GetByUserIdAsync(string userId)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.userId = @userId")
            .WithParameter("@userId", userId);

        var iterator = _container.GetItemQueryIterator<Order>(query);
        var results = new List<Order>();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            results.AddRange(response);
        }

        return results;
    }

    public async Task<List<Order>> GetAllAsync()
    {
        var query = new QueryDefinition("SELECT * FROM c ORDER BY c.createdAt DESC");

        var iterator = _container.GetItemQueryIterator<Order>(query);
        var results = new List<Order>();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            results.AddRange(response);
        }

        return results;
    }

    public async Task<Order> UpdateAsync(Order order)
    {
        var response = await _container.ReplaceItemAsync(order, order.Id, new PartitionKey(order.UserId));
        return response.Resource;
    }

    public async Task DeleteAsync(string id, string userId)
    {
        await _container.DeleteItemAsync<Order>(id, new PartitionKey(userId));
    }
}
