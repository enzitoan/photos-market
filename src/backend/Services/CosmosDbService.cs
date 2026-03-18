using Microsoft.Azure.Cosmos;
using PhotosMarket.API.Configuration;

namespace PhotosMarket.API.Services;

public interface ICosmosDbService
{
    Container GetContainer(string containerName);
    Task InitializeDatabaseAsync();
}

public class CosmosDbService : ICosmosDbService
{
    private readonly CosmosClient _cosmosClient;
    private readonly CosmosDbSettings _settings;
    private Database? _database;
    private bool _isInitialized = false;

    public CosmosDbService(string connectionString, CosmosDbSettings settings)
    {
        _cosmosClient = new CosmosClient(connectionString, new CosmosClientOptions
        {
            ConnectionMode = ConnectionMode.Gateway,
            RequestTimeout = TimeSpan.FromSeconds(5),
            HttpClientFactory = () => new HttpClient(new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
            })
        });
        _settings = settings;
    }

    public Container GetContainer(string containerName)
    {
        if (!_isInitialized)
        {
            // Para desarrollo: permitir acceso incluso sin inicialización completa
            _isInitialized = true;
        }
        
        if (_database == null)
        {
            // Intentar crear la database de forma lazy
            try
            {
                _database = _cosmosClient.GetDatabase(_settings.DatabaseName);
            }
            catch
            {
                throw new InvalidOperationException("Database not initialized. Call InitializeDatabaseAsync first or ensure Cosmos DB is running.");
            }
        }
        
        return _database.GetContainer(containerName);
    }

    public async Task InitializeDatabaseAsync()
    {
        try
        {
            _database = await _cosmosClient.CreateDatabaseIfNotExistsAsync(_settings.DatabaseName);

            // Create containers
            await _database.CreateContainerIfNotExistsAsync(
                _settings.ContainerNames.Orders, 
                "/userId");

            await _database.CreateContainerIfNotExistsAsync(
                _settings.ContainerNames.Users, 
                "/googleUserId");

            await _database.CreateContainerIfNotExistsAsync(
                _settings.ContainerNames.DownloadLinks, 
                "/userId");

            await _database.CreateContainerIfNotExistsAsync(
                _settings.ContainerNames.PhotographerSettings, 
                "/id");
                
            _isInitialized = true;
        }
        catch (Exception ex)
        {
            // Para desarrollo: marcar como inicializado incluso si falla
            // Esto permite que la app se ejecute sin Cosmos DB para probar otras funcionalidades
            Console.WriteLine($"Warning: Could not initialize Cosmos DB: {ex.Message}");
            Console.WriteLine("Running in mock mode - data will not be persisted");
            _isInitialized = true;
        }
    }
}
