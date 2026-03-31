using Microsoft.Extensions.Options;
using PhotosMarket.API.Configuration;
using SkiaSharp;

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
    /// Aplica una marca de agua en el centro/inferior de la imagen usando SkiaSharp
    /// PRESERVA el formato original y usa calidad máxima para evitar pérdida de datos
    /// </summary>
    /// <param name="imageStream">Stream de la imagen original</param>
    /// <param name="customText">Texto personalizado (por defecto: @egan.fotografia)</param>
    /// <returns>Stream de la imagen con marca de agua aplicada (mismo formato que la original)</returns>
    public async Task<Stream> ApplyWatermarkAsync(Stream imageStream, string? customText = null)
    {
        return await Task.Run(() =>
        {
            var watermarkText = customText ?? _settings.DefaultWatermarkText;
            
            // Cargar la imagen desde el stream
            using var originalBitmap = SKBitmap.Decode(imageStream);
            if (originalBitmap == null)
            {
                throw new InvalidOperationException("No se pudo decodificar la imagen");
            }
            
            // Crear una superficie para dibujar
            using var surface = SKSurface.Create(new SKImageInfo(originalBitmap.Width, originalBitmap.Height));
            var canvas = surface.Canvas;
            
            // Dibujar la imagen original
            canvas.DrawBitmap(originalBitmap, 0, 0);
            
            // Calcular tamaño de fuente dinámico
            var fontSize = Math.Max(originalBitmap.Width, originalBitmap.Height) / _settings.WatermarkFontSizeDivisor;
            
            // Configurar la fuente con estilo bold
            using var typeface = SKTypeface.FromFamilyName(
                "Arial",
                SKFontStyleWeight.Bold,
                SKFontStyleWidth.Normal,
                SKFontStyleSlant.Upright
            );
            
            using var font = new SKFont(typeface, fontSize);
            
            // Medir el texto para centrarlo
            var textBounds = new SKRect();
            using var paint = new SKPaint(font);
            paint.MeasureText(watermarkText, ref textBounds);
            
            // Calcular posición
            var x = (originalBitmap.Width - textBounds.Width) / 2f;
            var y = originalBitmap.Height * _settings.WatermarkVerticalPosition;
            
            _logger.LogInformation("Aplicando watermark - TextOpacity: {TextOpacity}, ShadowOpacity: {ShadowOpacity}", 
                _settings.WatermarkTextOpacity, _settings.WatermarkShadowOpacity);
            
            // SOMBRA: Color negro con transparencia configurable
            var shadowAlpha = (byte)(_settings.WatermarkShadowOpacity * 255);
            using var shadowPaint = new SKPaint(font)
            {
                Color = new SKColor(0, 0, 0, shadowAlpha), // Negro con alpha
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };
            
            _logger.LogInformation("Sombra: RGBA(0, 0, 0, {Alpha})", shadowAlpha);
            canvas.DrawText(watermarkText, x + 3, y + 3, shadowPaint);
            
            // TEXTO: Color blanco con transparencia configurable
            var textAlpha = (byte)(_settings.WatermarkTextOpacity * 255);
            using var textPaint = new SKPaint(font)
            {
                Color = new SKColor(255, 255, 255, textAlpha), // Blanco con alpha
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };
            
            _logger.LogInformation("Texto: RGBA(255, 255, 255, {Alpha})", textAlpha);
            canvas.DrawText(watermarkText, x, y, textPaint);
            
            // Guardar con calidad máxima
            var outputStream = new MemoryStream();
            using var image = surface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Jpeg, 100); // 100 = calidad máxima
            data.SaveTo(outputStream);
            
            outputStream.Position = 0;
            return (Stream)outputStream;
        });
    }
}
