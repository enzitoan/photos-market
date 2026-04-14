using Microsoft.Extensions.Options;
using PhotosMarket.API.Configuration;
using SkiaSharp;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;

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
            Stream? temporaryStream = null;
            var sourceStream = imageStream;

            if (!imageStream.CanSeek)
            {
                temporaryStream = new MemoryStream();
                imageStream.CopyTo(temporaryStream);
                temporaryStream.Position = 0;
                sourceStream = temporaryStream;
            }

            try
            {
                sourceStream.Position = 0;
                int rotationAngle = 0;
                try
                {
                    rotationAngle = GetExifOrientationAngle(sourceStream);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("No se pudieron leer metadatos EXIF: {Message}", ex.Message);
                }

                sourceStream.Position = 0;
                using var codec = SKCodec.Create(sourceStream);
                var imageFormat = codec?.EncodedFormat ?? SKEncodedImageFormat.Jpeg;
                sourceStream.Position = 0;

                using var originalBitmap = SKBitmap.Decode(sourceStream);
                if (originalBitmap == null)
                {
                    throw new InvalidOperationException("No se pudo decodificar la imagen");
                }

                SKBitmap? rotatedBitmap = null;
                SKBitmap bitmapToUse = originalBitmap;
                if (rotationAngle != 0)
                {
                    rotatedBitmap = RotateBitmap(originalBitmap, rotationAngle);
                    bitmapToUse = rotatedBitmap;
                }

                using var surface = SKSurface.Create(new SKImageInfo(bitmapToUse.Width, bitmapToUse.Height, bitmapToUse.ColorType, bitmapToUse.AlphaType));
                var canvas = surface.Canvas;
                canvas.Clear(SKColors.Transparent);
                canvas.DrawBitmap(bitmapToUse, 0, 0);

                var fontSize = Math.Max(bitmapToUse.Width, bitmapToUse.Height) / _settings.WatermarkFontSizeDivisor;
                using var typeface = SKTypeface.FromFamilyName(
                    "Arial",
                    SKFontStyleWeight.Bold,
                    SKFontStyleWidth.Normal,
                    SKFontStyleSlant.Upright
                );

                using var font = new SKFont(typeface, fontSize);
                var textBounds = new SKRect();
                using var paint = new SKPaint(font);
                paint.MeasureText(watermarkText, ref textBounds);

                var x = (bitmapToUse.Width - textBounds.Width) / 2f;
                var y = bitmapToUse.Height * _settings.WatermarkVerticalPosition;

                _logger.LogInformation("Aplicando watermark - TextOpacity: {TextOpacity}, ShadowOpacity: {ShadowOpacity}",
                    _settings.WatermarkTextOpacity, _settings.WatermarkShadowOpacity);

                var shadowAlpha = (byte)(_settings.WatermarkShadowOpacity * 255);
                using var shadowPaint = new SKPaint(font)
                {
                    Color = new SKColor(0, 0, 0, shadowAlpha),
                    IsAntialias = true,
                    Style = SKPaintStyle.Fill
                };

                _logger.LogInformation("Sombra: RGBA(0, 0, 0, {Alpha})", shadowAlpha);
                canvas.DrawText(watermarkText, x + 3, y + 3, shadowPaint);

                var textAlpha = (byte)(_settings.WatermarkTextOpacity * 255);
                using var textPaint = new SKPaint(font)
                {
                    Color = new SKColor(255, 255, 255, textAlpha),
                    IsAntialias = true,
                    Style = SKPaintStyle.Fill
                };

                _logger.LogInformation("Texto: RGBA(255, 255, 255, {Alpha})", textAlpha);
                canvas.DrawText(watermarkText, x, y, textPaint);

                var outputStream = new MemoryStream();
                using var image = surface.Snapshot();
                using var data = image.Encode(imageFormat, 100);
                data.SaveTo(outputStream);

                rotatedBitmap?.Dispose();
                outputStream.Position = 0;
                return (Stream)outputStream;
            }
            finally
            {
                temporaryStream?.Dispose();
            }
        });
    }

    private static int GetExifOrientationAngle(Stream stream)
    {
        var directories = ImageMetadataReader.ReadMetadata(stream);
        var exifDir = directories.OfType<ExifIfd0Directory>().FirstOrDefault();
        if (exifDir != null && exifDir.TryGetInt32(ExifDirectoryBase.TagOrientation, out int orientation))
        {
            return orientation switch
            {
                3 => 180,
                6 => 90,
                8 => 270,
                _ => 0,
            };
        }

        return 0;
    }

    private static SKBitmap RotateBitmap(SKBitmap source, int rotationAngle)
    {
        if (rotationAngle == 0)
        {
            return source;
        }

        int newWidth = rotationAngle == 90 || rotationAngle == 270 ? source.Height : source.Width;
        int newHeight = rotationAngle == 90 || rotationAngle == 270 ? source.Width : source.Height;
        var rotated = new SKBitmap(new SKImageInfo(newWidth, newHeight, source.ColorType, source.AlphaType));

        using var surface = SKSurface.Create(new SKImageInfo(newWidth, newHeight, source.ColorType, source.AlphaType));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        if (rotationAngle == 90)
        {
            canvas.Translate(newWidth, 0);
            canvas.RotateDegrees(90);
        }
        else if (rotationAngle == 180)
        {
            canvas.Translate(newWidth, newHeight);
            canvas.RotateDegrees(180);
        }
        else if (rotationAngle == 270)
        {
            canvas.Translate(0, newHeight);
            canvas.RotateDegrees(270);
        }

        canvas.DrawBitmap(source, 0, 0);
        using var image = surface.Snapshot();
        image.ReadPixels(rotated.Info, rotated.GetPixels(), rotated.RowBytes, 0, 0);

        return rotated;
    }
}
