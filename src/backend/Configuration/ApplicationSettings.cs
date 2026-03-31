namespace PhotosMarket.API.Configuration;

public class ApplicationSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string FrontendUrl { get; set; } = string.Empty;
    public int DownloadLinkExpirationHours { get; set; } = 72;
    public string WatermarkText { get; set; } = string.Empty;
    public string DefaultWatermarkText { get; set; } = "@egan.fotografia";
    public decimal PhotoPricePerUnit { get; set; } = 1000m;
    public string Currency { get; set; } = "CLP";
    
    // Discount Configuration
    public int BulkDiscountMinPhotos { get; set; } = 5;
    public decimal BulkDiscountPercentage { get; set; } = 20m;
    
    // Watermark Configuration
    /// <summary>
    /// Divisor del tamaño de la imagen para calcular el tamaño de fuente.
    /// Menor = marca más grande. Ejemplo: 20 = ~5% del tamaño, 30 = ~3%, 40 = ~2.5%
    /// </summary>
    public float WatermarkFontSizeDivisor { get; set; } = 55f;
    
    /// <summary>
    /// Opacidad del texto principal (0.0 = transparente, 1.0 = opaco)
    /// Valores recomendados: 0.6 - 0.9
    /// </summary>
    public float WatermarkTextOpacity { get; set; } = 0.6f;
    
    /// <summary>
    /// Opacidad de la sombra del texto (0.0 = transparente, 1.0 = opaco)
    /// Valores recomendados: 0.3 - 0.5
    /// </summary>
    public float WatermarkShadowOpacity { get; set; } = 0.3f;
    
    /// <summary>
    /// Posición vertical del watermark (0.0 = arriba, 1.0 = abajo)
    /// Valores recomendados: 0.85 - 0.95 para parte inferior
    /// </summary>
    public float WatermarkVerticalPosition { get; set; } = 0.9f;
}
