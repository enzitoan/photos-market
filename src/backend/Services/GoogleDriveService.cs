using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using PhotosMarket.API.Configuration;
using PhotosMarket.API.DTOs;
using System.Collections.Concurrent;
using System.Text;

namespace PhotosMarket.API.Services;

public class GoogleDriveService
{
    private const int MaxParallelDriveRequests = 5;

    private readonly GoogleDriveSettings _settings;
    private readonly ILogger<GoogleDriveService> _logger;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheDuration;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _cacheLocks = new();
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private CancellationTokenSource _cacheResetSource = new();
    private DriveService? _driveService;

    public GoogleDriveService(
        GoogleDriveSettings settings,
        IMemoryCache cache,
        ILogger<GoogleDriveService> logger)
    {
        _settings = settings;
        _cache = cache;
        _logger = logger;
        _cacheDuration = TimeSpan.FromMinutes(Math.Max(0, settings.CacheMinutes));
    }

    /// <summary>
    /// Devuelve el servicio de Drive, inicializándolo una sola vez aunque haya llamadas concurrentes
    /// </summary>
    private async Task<DriveService> GetDriveServiceAsync()
    {
        if (_driveService != null)
            return _driveService;

        await _initLock.WaitAsync();
        try
        {
            return _driveService ?? await CreateDriveServiceAsync();
        }
        finally
        {
            _initLock.Release();
        }
    }

    /// <summary>
    /// Inicializa el servicio de Google Drive usando credenciales de Service Account
    /// </summary>
    private async Task<DriveService> CreateDriveServiceAsync()
    {
        try
        {
            GoogleCredential credential;

            // Prioridad 1: Usar CredentialsJson (desde Key Vault en producción)
            if (!string.IsNullOrEmpty(_settings.CredentialsJson))
            {
                _logger.LogInformation("Using Google Drive credentials from environment variable (Key Vault)");
                
                credential = await Task.Run(() =>
                {
                    using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(_settings.CredentialsJson)))
                    {
                        return GoogleCredential.FromStream(stream)
                            .CreateScoped(DriveService.Scope.DriveReadonly);
                    }
                });
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
                    credential = (await GoogleCredential.FromStreamAsync(stream, cancellationToken: CancellationToken.None))
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
    /// Obtiene todos los álbumes (carpetas) del directorio raíz.
    /// El resultado se cachea en memoria; los consumidores no deben modificarlo.
    /// </summary>
    public async Task<IReadOnlyList<AlbumDto>> GetAlbumsAsync()
    {
        try
        {
            var hadErrors = false;

            var albums = await GetOrCreateCachedAsync<IReadOnlyList<AlbumDto>>(
                AlbumsCacheKey,
                async () =>
                {
                    var service = await GetDriveServiceAsync();

                    // Listar todas las carpetas dentro de la carpeta raíz
                    var folders = new List<Google.Apis.Drive.v3.Data.File>();
                    string? pageToken = null;
                    do
                    {
                        var request = service.Files.List();
                        request.Q = $"'{_settings.RootFolderId}' in parents and mimeType='application/vnd.google-apps.folder' and trashed=false";
                        request.Fields = "nextPageToken, files(id, name, createdTime)";
                        request.OrderBy = "name";
                        request.PageSize = 1000;
                        request.PageToken = pageToken;

                        var result = await request.ExecuteAsync();
                        if (result.Files != null)
                            folders.AddRange(result.Files);
                        pageToken = result.NextPageToken;
                    } while (!string.IsNullOrEmpty(pageToken));

                    // Resumen (conteo + portada) de cada álbum en paralelo, con concurrencia limitada
                    using var throttle = new SemaphoreSlim(MaxParallelDriveRequests);
                    var tasks = folders.Select(async folder =>
                    {
                        // Validar que la carpeta tenga datos válidos
                        if (folder == null || string.IsNullOrEmpty(folder.Id) || string.IsNullOrEmpty(folder.Name))
                        {
                            _logger.LogWarning("Carpeta con datos inválidos encontrada, omitiendo...");
                            return null;
                        }

                        await throttle.WaitAsync();
                        try
                        {
                            var (photosCount, coverUrl) = await GetAlbumSummaryAsync(folder.Id);

                            return new AlbumDto
                            {
                                Id = folder.Id,
                                Title = folder.Name,
                                MediaItemsCount = photosCount,
                                CreatedAt = folder.CreatedTimeDateTimeOffset?.UtcDateTime,
                                CoverPhotoUrl = coverUrl ?? ""
                            };
                        }
                        catch (Exception ex)
                        {
                            hadErrors = true;
                            _logger.LogWarning(ex, "Error procesando carpeta {FolderId}, omitiendo...", folder.Id);
                            return null;
                        }
                        finally
                        {
                            throttle.Release();
                        }
                    });

                    var list = (await Task.WhenAll(tasks))
                        .Where(a => a != null)
                        .Select(a => a!)
                        .ToList();

                    // Reutilizar el resumen para las consultas por ID
                    if (!hadErrors)
                    {
                        foreach (var album in list)
                            SetCache(AlbumCacheKey(album.Id), album);
                    }

                    _logger.LogInformation("Found {Count} albums in Google Drive", list.Count);
                    return (IReadOnlyList<AlbumDto>?)list;
                },
                shouldCache: _ => !hadErrors);

            return albums ?? new List<AlbumDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener álbumes de Google Drive");
            throw;
        }
    }

    /// <summary>
    /// Obtiene las fotos de un álbum específico.
    /// El resultado se cachea en memoria; los consumidores no deben modificarlo.
    /// </summary>
    public async Task<IReadOnlyList<PhotoDto>> GetPhotosFromAlbumAsync(string albumId)
    {
        try
        {
            var photos = await GetOrCreateCachedAsync<IReadOnlyList<PhotoDto>>(
                PhotosCacheKey(albumId),
                async () =>
                {
                    var service = await GetDriveServiceAsync();
                    var list = new List<PhotoDto>();
                    string? pageToken = null;

                    // Listar todos los archivos de imagen en la carpeta (con paginación)
                    do
                    {
                        var request = service.Files.List();
                        request.Q = ImagesInFolderQuery(albumId);
                        request.Fields = "nextPageToken, files(id, name, createdTime)";
                        request.OrderBy = "name";
                        request.PageSize = 1000; // Máximo permitido
                        request.PageToken = pageToken;

                        var result = await request.ExecuteAsync();

                        if (result.Files != null)
                        {
                            list.AddRange(result.Files.Select(file => new PhotoDto
                            {
                                Id = file.Id,
                                MediaItemId = file.Id,
                                Filename = file.Name,
                                ThumbnailUrl = GetGoogleDriveThumbnailUrl(file.Id, null),
                                BaseUrl = GetGoogleDriveDirectUrl(file.Id),
                                CreationTime = file.CreatedTimeDateTimeOffset?.UtcDateTime
                            }));
                        }

                        pageToken = result.NextPageToken;
                    } while (!string.IsNullOrEmpty(pageToken));

                    _logger.LogInformation("Found {Count} photos in album {AlbumId}", list.Count, albumId);
                    return (IReadOnlyList<PhotoDto>?)list;
                });

            return photos ?? new List<PhotoDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener fotos del álbum {AlbumId}", albumId);
            throw;
        }
    }

    /// <summary>
    /// Descarta toda la información cacheada de Drive (álbumes, portadas y fotos)
    /// </summary>
    public void InvalidateCache()
    {
        var previous = Interlocked.Exchange(ref _cacheResetSource, new CancellationTokenSource());
        previous.Cancel();
        previous.Dispose();
        _logger.LogInformation("Google Drive cache invalidated");
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
    /// Obtiene la cantidad de fotos y la URL de portada (primera foto) de una carpeta
    /// con una única consulta paginada
    /// </summary>
    private async Task<(int Count, string? CoverUrl)> GetAlbumSummaryAsync(string folderId)
    {
        var service = await GetDriveServiceAsync();
        var count = 0;
        string? firstFileId = null;
        string? pageToken = null;

        do
        {
            var request = service.Files.List();
            request.Q = ImagesInFolderQuery(folderId);
            request.Fields = "nextPageToken, files(id)";
            request.OrderBy = "name";
            request.PageSize = 1000;
            request.PageToken = pageToken;

            var result = await request.ExecuteAsync();
            var files = result.Files ?? new List<Google.Apis.Drive.v3.Data.File>();

            firstFileId ??= files.FirstOrDefault()?.Id;
            count += files.Count;
            pageToken = result.NextPageToken;
        } while (!string.IsNullOrEmpty(pageToken));

        var coverUrl = firstFileId == null ? null : GetGoogleDriveThumbnailUrl(firstFileId, null);
        return (count, coverUrl);
    }

    /// <summary>
    /// Obtiene información de un álbum específico por ID
    /// </summary>
    public async Task<AlbumDto?> GetAlbumByIdAsync(string albumId)
    {
        try
        {
            return await GetOrCreateCachedAsync<AlbumDto>(AlbumCacheKey(albumId), async () =>
            {
                var service = await GetDriveServiceAsync();

                // Obtener información de la carpeta
                var request = service.Files.Get(albumId);
                request.Fields = "id, name, createdTime";

                var folder = await request.ExecuteAsync();

                if (folder == null || string.IsNullOrEmpty(folder.Id))
                {
                    return null;
                }

                var (photosCount, coverUrl) = await GetAlbumSummaryAsync(folder.Id);

                return new AlbumDto
                {
                    Id = folder.Id,
                    Title = folder.Name,
                    MediaItemsCount = photosCount,
                    CreatedAt = folder.CreatedTimeDateTimeOffset?.UtcDateTime,
                    CoverPhotoUrl = coverUrl ?? ""
                };
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener álbum {AlbumId}", albumId);
            return null;
        }
    }

    /// <summary>
    /// Obtiene solo el título de un álbum (evita calcular conteo y portada)
    /// </summary>
    public async Task<string?> GetAlbumTitleAsync(string albumId)
    {
        try
        {
            if (_cache.TryGetValue(AlbumCacheKey(albumId), out AlbumDto? cachedAlbum) && cachedAlbum != null)
                return cachedAlbum.Title;

            return await GetOrCreateCachedAsync<string>(AlbumTitleCacheKey(albumId), async () =>
            {
                var service = await GetDriveServiceAsync();
                var request = service.Files.Get(albumId);
                request.Fields = "id, name";

                var folder = await request.ExecuteAsync();
                return string.IsNullOrEmpty(folder?.Name) ? null : folder.Name;
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al obtener título del álbum {AlbumId}", albumId);
            return null;
        }
    }

    private static string ImagesInFolderQuery(string folderId) =>
        $"'{folderId}' in parents and trashed=false and mimeType contains 'image/'";

    private static string AlbumsCacheKey => "drive:albums";
    private static string AlbumCacheKey(string albumId) => $"drive:album:{albumId}";
    private static string AlbumTitleCacheKey(string albumId) => $"drive:album-title:{albumId}";
    private static string PhotosCacheKey(string albumId) => $"drive:photos:{albumId}";

    private void SetCache<T>(string key, T value) where T : class
    {
        if (_cacheDuration <= TimeSpan.Zero)
            return;

        var options = new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = _cacheDuration };
        options.AddExpirationToken(new CancellationChangeToken(_cacheResetSource.Token));
        _cache.Set(key, value, options);
    }

    /// <summary>
    /// Devuelve el valor cacheado o ejecuta la fábrica una sola vez por clave aunque haya
    /// peticiones concurrentes. Los valores nulos y los resultados rechazados por
    /// <paramref name="shouldCache"/> no se almacenan.
    /// </summary>
    private async Task<T?> GetOrCreateCachedAsync<T>(
        string key,
        Func<Task<T?>> factory,
        Func<T, bool>? shouldCache = null) where T : class
    {
        if (_cache.TryGetValue(key, out T? cached))
            return cached;

        var gate = _cacheLocks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync();
        try
        {
            if (_cache.TryGetValue(key, out cached))
                return cached;

            var value = await factory();
            if (value != null && (shouldCache?.Invoke(value) ?? true))
                SetCache(key, value);

            return value;
        }
        finally
        {
            gate.Release();
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
