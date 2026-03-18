namespace PhotosMarket.API.Configuration;

public class GoogleDriveSettings
{
    /// <summary>
    /// Ruta al archivo de credenciales JSON de la Service Account (desarrollo local)
    /// </summary>
    public string CredentialsFilePath { get; set; } = "google-drive-credentials.json";
    
    /// <summary>
    /// Credenciales JSON de la Service Account (producción - desde Key Vault)
    /// </summary>
    public string? CredentialsJson { get; set; }
    
    /// <summary>
    /// ID de la carpeta raíz en Google Drive que contiene los álbumes
    /// Cada subcarpeta será tratada como un álbum
    /// </summary>
    public string RootFolderId { get; set; } = string.Empty;
    
    /// <summary>
    /// Nombre de la aplicación para Google Drive API
    /// </summary>
    public string ApplicationName { get; set; } = "PhotosMarket";
}
