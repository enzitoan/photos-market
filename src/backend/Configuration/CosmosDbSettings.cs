namespace PhotosMarket.API.Configuration;

public class CosmosDbSettings
{
    public string DatabaseName { get; set; } = string.Empty;
    public ContainerNamesSettings ContainerNames { get; set; } = new();
}

public class ContainerNamesSettings
{
    public string Orders { get; set; } = "Orders";
    public string Users { get; set; } = "Users";
    public string DownloadLinks { get; set; } = "DownloadLinks";
    public string PhotographerSettings { get; set; } = "PhotographerSettings";
    public string Albums { get; set; } = "Albums";
}
