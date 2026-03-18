using Microsoft.Azure.Cosmos;
using PhotosMarket.API.Configuration;
using PhotosMarket.API.Services;
using Microsoft.Extensions.Options;
using UserModel = PhotosMarket.API.Models.User;

namespace PhotosMarket.API.Repositories;

public interface IUserRepository
{
    Task<UserModel> CreateAsync(UserModel user);
    Task<UserModel?> GetByIdAsync(string id);
    Task<UserModel?> GetByGoogleUserIdAsync(string googleUserId);
    Task<UserModel?> GetByEmailAsync(string email);
    Task<UserModel> UpdateAsync(UserModel user);
}

public class UserRepository : IUserRepository
{
    private readonly Container _container;

    public UserRepository(ICosmosDbService cosmosDbService, IOptions<CosmosDbSettings> settings)
    {
        _container = cosmosDbService.GetContainer(settings.Value.ContainerNames.Users);
    }

    public async Task<UserModel> CreateAsync(UserModel user)
    {
        var response = await _container.CreateItemAsync(user, new PartitionKey(user.GoogleUserId));
        return response.Resource;
    }

    public async Task<UserModel?> GetByIdAsync(string id)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.id = @id")
            .WithParameter("@id", id);

        var iterator = _container.GetItemQueryIterator<UserModel>(query);
        
        if (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            return response.FirstOrDefault();
        }

        return null;
    }

    public async Task<UserModel?> GetByGoogleUserIdAsync(string googleUserId)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.googleUserId = @googleUserId")
            .WithParameter("@googleUserId", googleUserId);

        var iterator = _container.GetItemQueryIterator<UserModel>(query);
        
        if (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            return response.FirstOrDefault();
        }

        return null;
    }

    public async Task<UserModel?> GetByEmailAsync(string email)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.email = @email")
            .WithParameter("@email", email);

        var iterator = _container.GetItemQueryIterator<UserModel>(query);
        
        if (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            return response.FirstOrDefault();
        }

        return null;
    }

    public async Task<UserModel> UpdateAsync(UserModel user)
    {
        var response = await _container.ReplaceItemAsync(user, user.Id, new PartitionKey(user.GoogleUserId));
        return response.Resource;
    }
}
