using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using PhotosMarket.API.Configuration;
using PhotosMarket.API.DTOs;
using System.Text;

namespace PhotosMarket.API.Services;

public class GoogleDriveService
{
    private readonly GoogleDriveSettings _settings;
    private readonly ILogger<GoogleDriveService> _logger;
    private DriveService? _driveService;

    public GoogleDriveService(
        GoogleDriveSettings settings,
        ILogger<GoogleDriveService> logger)
    {
        _settings = settings;
        _logger = logger;
    }

    /// <summary>
    /// Inicializa el servicio de Google Drive usando credenciales de Service Account
    /// </summary>
    private async Task<DriveService> GetDriveServiceAsync()
    {
        if (_driveService != null)
            return _driveService;

        try
        {
            GoogleCredential credential;

            // Prioridad 1: Usar CredentialsJson (desde Key Vault en producción)
            if (!string.IsNullOrEmpty(_settings.CredentialsJson))
            {
                _logger.LogInformation("Using Google Drive credentials from environment variable (Key Vault)");
                
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(_settings.CredentialsJson)))
                {
                    credential = GoogleCredential.FromStream(stream)
                        .CreateScoped(DriveService.Scope.DriveReadonly);
                }
            }
            // Prioridad 2: Usar archivo local (desarrollo)
            else
            {
                var credentialsPath = Path.Combine(Directory.GetCurrentDirectory(), _settings.CredentialsFilePath);
                _logger.LogInformation($"Using Google Drive credentials from file: {credentialsPath}");

                if (!System.IO.File.Exists(credentialsPath))
                {
                    throw new FileNotFoundException(
                        $"Archivo de credenciales no encontrado: {credentialsPath}. " +
                        "Descarga el archivo JSON de la Service Account y colócalo en src/backend/ o configura la variable de entorno GoogleDrive__CredentialsJson"
                    );
                }

                using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
                {
                    credential = GoogleCredential.FromStream(stream)
                        .CreateScoped(DriveService.Scope.DriveReadonly);
                }
            }

            _driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = _settings.ApplicationName,
            });

            _logger.LogInformation("GoogleDriveService initialized successfully");
            return _driveService;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al inicializar Google Drive Service");
            throw;
        }
    }

    /// <summary>
    /// Obtiene todos los álbumes (carpetas) del directorio raíz
    /// </summary>
    public async Task<List<AlbumDto>> GetAlbumsAsync()
    {
        try
        {
            var service = await GetDriveServiceAsync();

            // Listar todas las carpetas dentro de la carpeta raíz
            var request = service.Files.List();
            request.Q = $"'{_settings.RootFolderId}' in parents and mimeType='application/vnd.google-apps.folder' and trashed=false";
            request.Fields = "files(id, name, webViewLink, createdTime, modifiedTime)";
            request.OrderBy = "name";

            var result = await request.ExecuteAsync();

            var albums = new List<AlbumDto>();

            if (result.Files != null)
            {
                foreach (var folder in result.Files)
                {
                    // Validar que la carpeta tenga datos válidos
                    if (folder == null || string.IsNullOrEmpty(folder.Id) || string.IsNullOrEmpty(folder.Name))
                    {
                        _logger.LogWarning("Carpeta con datos inválidos encontrada, omitiendo...");
                        continue;
                    }

                    try
                    {
                        // Contar cuántas fotos hay en esta carpeta
                        var photosCount = await GetPhotosCountAsync(folder.Id);

                        albums.Add(new AlbumDto
                        {
                            Id = folder.Id,
                            Title = folder.Name,
                            MediaItemsCount = photosCount,
                            CoverPhotoUrl = await GetFirstPhotoThumbnailAsync(folder.Id) ?? ""
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error procesando carpeta {FolderId}, omitiendo...", folder.Id);
                    }
                }
            }

            _logger.LogInformation("Found {Count} albums in Google Drive", albums.Count);
            return albums;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener álbumes de Google Drive");
            throw;
        }
    }

    /// <summary>
    /// Obtiene las fotos de un álbum específico
    /// </summary>
    public async Task<List<PhotoDto>> GetPhotosFromAlbumAsync(string albumId)
    {
        try
        {
            var service = await GetDriveServiceAsync();

            // Listar todos los archivos de imagen en la carpeta
            var request = service.Files.List();
            request.Q = $"'{albumId}' in parents and trashed=false and (mimeType contains 'image/' or mimeType='image/jpeg' or mimeType='image/png' or mimeType='image/jpg')";
            request.Fields = "files(id, name, webViewLink, webContentLink, thumbnailLink, mimeType, size, createdTime)";
            request.OrderBy = "name";
            request.PageSize = 1000; // Máximo permitido

            var result = await request.ExecuteAsync();

            var photos = result.Files.Select(file => new PhotoDto
            {
                Id = file.Id,
                MediaItemId = file.Id,
                Filename = file.Name,
                // Usar el endpoint proxy del backend como fallback
                ThumbnailUrl = GetGoogleDriveThumbnailUrl(file.Id, file.ThumbnailLink),
                BaseUrl = GetGoogleDriveDirectUrl(file.Id),
                CreationTime = file.CreatedTimeDateTimeOffset?.UtcDateTime
            }).ToList();

            _logger.LogInformation("Found {Count} photos in album {AlbumId}", photos.Count, albumId);
            return photos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener fotos del álbum {AlbumId}", albumId);
            throw;
        }
    }

    /// <summary>
    /// Obtiene la URL de descarga directa de una foto
    /// </summary>
    public async Task<string> GetPhotoDownloadUrlAsync(string photoId)
    {
        try
        {
            var service = await GetDriveServiceAsync();

            var request = service.Files.Get(photoId);
            request.Fields = "webContentLink, id";
            var file = await request.ExecuteAsync();

            // Si el archivo tiene webContentLink, usarlo
            if (!string.IsNullOrEmpty(file.WebContentLink))
            {
                return file.WebContentLink;
            }

            // Si no, construir URL de descarga directa
            return $"https://drive.google.com/uc?export=download&id={photoId}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener URL de descarga para foto {PhotoId}", photoId);
            throw;
        }
    }

    /// <summary>
    /// Descarga una foto y retorna el stream
    /// </summary>
    public async Task<Stream> DownloadPhotoAsync(string photoId)
    {
        try
        {
            var service = await GetDriveServiceAsync();

            var request = service.Files.Get(photoId);
            var stream = new MemoryStream();

            await request.DownloadAsync(stream);
            stream.Position = 0;

            _logger.LogInformation("Downloaded photo {PhotoId}, size: {Size} bytes", photoId, stream.Length);
            return stream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al descargar foto {PhotoId}", photoId);
            throw;
        }
    }

    /// <summary>
    /// Obtiene los metadatos de una foto
    /// </summary>
    public async Task<Google.Apis.Drive.v3.Data.File> GetPhotoMetadataAsync(string photoId)
    {
        try
        {
            var service = await GetDriveServiceAsync();

            var request = service.Files.Get(photoId);
            request.Fields = "id, name, mimeType, size, webContentLink, thumbnailLink, createdTime";
            
            return await request.ExecuteAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener metadata de foto {PhotoId}", photoId);
            throw;
        }
    }

    /// <summary>
    /// Cuenta cuántas fotos hay en una carpeta
    /// </summary>
    private async Task<int> GetPhotosCountAsync(string folderId)
    {
        try
        {
            var service = await GetDriveServiceAsync();

            var request = service.Files.List();
            request.Q = $"'{folderId}' in parents and trashed=false and (mimeType contains 'image/')";
            request.Fields = "files(id)";
            request.PageSize = 1000;

            var result = await request.ExecuteAsync();
            return result.Files.Count;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al contar fotos en carpeta {FolderId}", folderId);
            return 0;
        }
    }

    /// <summary>
    /// Obtiene la URL del thumbnail de la primera foto de una carpeta (para cover del álbum)
    /// </summary>
    private async Task<string?> GetFirstPhotoThumbnailAsync(string folderId)
    {
        try
        {
            var service = await GetDriveServiceAsync();

            var request = service.Files.List();
            request.Q = $"'{folderId}' in parents and trashed=false and (mimeType contains 'image/')";
            request.Fields = "files(id, thumbnailLink)";
            request.PageSize = 1;
            request.OrderBy = "name";

            var result = await request.ExecuteAsync();
            var firstFile = result.Files.FirstOrDefault();
            
            if (firstFile == null)
                return null;
            
            // Usar el mismo método que para las fotos individuales para generar URL pública
            return GetGoogleDriveThumbnailUrl(firstFile.Id, firstFile.ThumbnailLink);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al obtener thumbnail de carpeta {FolderId}", folderId);
            return null;
        }
    }
    
    /// <summary>
    /// Obtiene información de un álbum específico por ID
    /// </summary>
    public async Task<AlbumDto?> GetAlbumByIdAsync(string albumId)
    {
        try
        {
            var service = await GetDriveServiceAsync();

            // Obtener información de la carpeta
            var request = service.Files.Get(albumId);
            request.Fields = "id, name, webViewLink, createdTime, modifiedTime";
            
            var folder = await request.ExecuteAsync();

            if (folder == null || string.IsNullOrEmpty(folder.Id))
            {
                return null;
            }

            var photosCount = await GetPhotosCountAsync(folder.Id);
            var coverPhotoUrl = await GetFirstPhotoThumbnailAsync(folder.Id);

            return new AlbumDto
            {
                Id = folder.Id,
                Title = folder.Name,
                MediaItemsCount = photosCount,
                CoverPhotoUrl = coverPhotoUrl ?? ""
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener álbum {AlbumId}", albumId);
            return null;
        }
    }
    
    /// <summary>
    /// Genera una URL de thumbnail accesible para Google Drive
    /// </summary>
    private string GetGoogleDriveThumbnailUrl(string fileId, string? originalThumbnailLink)
    {
        // IMPORTANTE: Para que las URLs funcionen, los archivos deben estar compartidos públicamente
        // o la carpeta debe estar compartida con "Anyone with the link can view"
        // Usar la URL de thumbnail de Google Drive que funciona con archivos compartidos
        return $"https://lh3.googleusercontent.com/d/{fileId}=w400";
    }
    
    /// <summary>
    /// Genera una URL de acceso directo para Google Drive
    /// </summary>
    private string GetGoogleDriveDirectUrl(string fileId)
    {
        // URL para visualizar imágenes con mayor resolución
        // Esta URL funciona si el archivo está compartido públicamente o con "Anyone with the link"
        return $"https://lh3.googleusercontent.com/d/{fileId}=w2000";
    }
}
