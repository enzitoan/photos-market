namespace PhotosMarket.API.Configuration;

public class CosmosDbSettings
{
    public string DatabaseName { get; set; } = string.Empty;
    public ContainerNamesSettings ContainerNames { get; set; } = new();
}

public class ContainerNamesSettings
{
    public string Orders { get; set; } = string.Empty;
    public string Users { get; set; } = string.Empty;
    public string DownloadLinks { get; set; } = string.Empty;
    public string PhotographerSettings { get; set; } = string.Empty;
}
