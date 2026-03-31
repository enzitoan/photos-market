using Microsoft.Extensions.Options;
using PhotosMarket.API.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Processing;

namespace PhotosMarket.API.Services;

public interface IWatermarkService
{
    string GetWatermarkText();
    Task<Stream> ApplyWatermarkAsync(Stream imageStream, string? customText = null);
}

public class WatermarkService : IWatermarkService
{
    private readonly ApplicationSettings _settings;
    private readonly ILogger<WatermarkService> _logger;

    public WatermarkService(IOptions<ApplicationSettings> settings, ILogger<WatermarkService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public string GetWatermarkText()
    {
        return _settings.WatermarkText.Replace("{YEAR}", DateTime.Now.Year.ToString());
    }

    /// <summary>
    /// Aplica una marca de agua en el centro/inferior de la imagen
    /// PRESERVA el formato original y usa calidad máxima para evitar pérdida de datos
    /// </summary>
    /// <param name="imageStream">Stream de la imagen original</param>
    /// <param name="customText">Texto personalizado (por defecto: @egan.fotografia)</param>
    /// <returns>Stream de la imagen con marca de agua aplicada (mismo formato que la original)</returns>
    public async Task<Stream> ApplyWatermarkAsync(Stream imageStream, string? customText = null)
    {
        var watermarkText = customText ?? _settings.DefaultWatermarkText;
        
        // Cargar la imagen Y detectar el formato original
        var imageInfo = await Image.IdentifyAsync(imageStream);
        imageStream.Position = 0; // Reset para cargar la imagen
        
        using var image = await Image.LoadAsync(imageStream);
        
        // Calcular tamaño de fuente dinámico basado en el tamaño de la imagen
        // Usar configuración para ajustar el tamaño (menor divisor = marca más grande)
        var fontSize = Math.Max(image.Width, image.Height) / _settings.WatermarkFontSizeDivisor;
        
        // Crear una fuente - priorizar Montserrat para marca consistente
        // Orden de prioridad: Montserrat -> Montserrat Bold -> Liberation Sans -> DejaVu Sans -> Arial -> cualquier Sans
        Font font;
        string fontUsed = "Unknown";
        try
        {
            font = SystemFonts.CreateFont("Montserrat", fontSize, FontStyle.Bold);
            fontUsed = "Montserrat";
        }
        catch
        {
            try
            {
                // Intentar variante explícita de Montserrat Bold
                font = SystemFonts.CreateFont("Montserrat Bold", fontSize, FontStyle.Bold);
                fontUsed = "Montserrat Bold";
            }
            catch
            {
                try
                {
                    font = SystemFonts.CreateFont("Liberation Sans", fontSize, FontStyle.Bold);
                    fontUsed = "Liberation Sans";
                }
                catch
                {
                    try
                    {
                        font = SystemFonts.CreateFont("DejaVu Sans", fontSize, FontStyle.Bold);
                        fontUsed = "DejaVu Sans";
                    }
                    catch
                    {
                        try
                        {
                            font = SystemFonts.CreateFont("Arial", fontSize, FontStyle.Bold);
                            fontUsed = "Arial";
                        }
                        catch
                        {
                            // Fallback: usar la primera fuente Sans disponible o cualquier fuente
                            var sansFont = SystemFonts.Families.FirstOrDefault(f => 
                                f.Name.Contains("Sans", StringComparison.OrdinalIgnoreCase));
                            
                            // Si hay una fuente Sans, usarla; si no, usar la primera disponible
                            var fontFamily = !string.IsNullOrEmpty(sansFont.Name) 
                                ? sansFont 
                                : SystemFonts.Families.First();
                            
                            font = fontFamily.CreateFont(fontSize, FontStyle.Bold);
                            fontUsed = fontFamily.Name;
                        }
                    }
                }
            }
        }
        
        _logger.LogInformation("Aplicando watermark con fuente: {Font}, tamaño: {Size}px", fontUsed, fontSize);
        
        // Calcular posición: centro horizontal, posición vertical configurable
        var textOptions = new RichTextOptions(font)
        {
            Origin = new PointF(0, 0),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        
        // Medir el texto para centrado
        var textBounds = TextMeasurer.MeasureBounds(watermarkText, textOptions);
        var x = image.Width / 2f;
        var y = image.Height * _settings.WatermarkVerticalPosition; // Posición configurable
        
        // Aplicar marca de agua con sombra para mejor visibilidad
        image.Mutate(ctx =>
        {
            // Sombra oscura para contraste (opacidad configurable)
            ctx.DrawText(
                new RichTextOptions(font)
                {
                    Origin = new PointF(x + 3, y + 3),
                    HorizontalAlignment = HorizontalAlignment.Center
                },
                watermarkText,
                Color.Black.WithAlpha(_settings.WatermarkShadowOpacity)
            );
            
            // Texto principal en blanco semi-transparente (opacidad configurable)
            ctx.DrawText(
                new RichTextOptions(font)
                {
                    Origin = new PointF(x, y),
                    HorizontalAlignment = HorizontalAlignment.Center
                },
                watermarkText,
                Color.White.WithAlpha(_settings.WatermarkTextOpacity)
            );
        });
        
        // Guardar en el MISMO FORMATO que la original con CALIDAD MÁXIMA
        var outputStream = new MemoryStream();
        
        // Determinar el encoder basado en el formato original
        var formatName = imageInfo?.Metadata?.DecodedImageFormat?.Name?.ToLowerInvariant();
        
        switch (formatName)
        {
            case "jpeg":
            case "jpg":
                // JPEG con calidad máxima (100 = sin pérdida visible)
                var jpegEncoder = new JpegEncoder
                {
                    Quality = 100 // Calidad máxima (1-100)
                };
                await image.SaveAsJpegAsync(outputStream, jpegEncoder);
                break;
                
            case "png":
                // PNG sin pérdida (compresión lossless)
                var pngEncoder = new PngEncoder
                {
                    CompressionLevel = PngCompressionLevel.BestCompression
                };
                await image.SaveAsPngAsync(outputStream, pngEncoder);
                break;
                
            default:
                // Para otros formatos (GIF, WEBP, BMP), usar JPEG calidad máxima como fallback
                var defaultEncoder = new JpegEncoder { Quality = 100 };
                await image.SaveAsJpegAsync(outputStream, defaultEncoder);
                break;
        }
        
        outputStream.Position = 0;
        
        return outputStream;
    }
}
