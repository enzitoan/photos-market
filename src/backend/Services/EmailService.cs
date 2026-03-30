using Microsoft.Extensions.Options;
using PhotosMarket.API.Configuration;
using PhotosMarket.API.Models;
using Resend;

namespace PhotosMarket.API.Services;

public interface IEmailService
{
    Task SendOrderAwaitingPaymentEmailAsync(Order order, string userEmail);
    Task SendPaymentConfirmedEmailAsync(Order order, string userEmail);
    Task SendOrderCompletedEmailAsync(Order order, string userEmail);
    Task SendPaymentConfirmedWithDownloadLinkAsync(Order order, DownloadLink downloadLink, string userEmail);
}


public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ApplicationSettings _appSettings;
    private readonly ILogger<EmailService> _logger;
    private readonly IResend _resend;

    public EmailService(
        IOptions<EmailSettings> settings,
        IOptions<ApplicationSettings> appSettings,
        ILogger<EmailService> logger,
        IResend resend)
    {
        _settings = settings.Value;
        _appSettings = appSettings.Value;
        _logger = logger;
        _resend = resend;
    }

    public async Task SendOrderAwaitingPaymentEmailAsync(Order order, string userEmail)
    {
        if (!_settings.Enabled)
        {
            _logger.LogInformation("Email service is disabled. Skipping awaiting payment email for order {OrderId}", order.Id);
            return;
        }

        // Construir sección de descuento si aplica
        var discountSection = "";
        if (order.DiscountPercentage.HasValue && order.DiscountPercentage.Value > 0)
        {
            discountSection = $@"
                        <li><strong>Subtotal:</strong> {order.Currency} {order.Subtotal:F2}</li>
                        <li style='color: green;'><strong>Descuento ({order.DiscountPercentage.Value}%):</strong> -{order.Currency} {order.DiscountAmount:F2}</li>";
        }

        var htmlBody = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>¡Gracias por tu pedido!</h2>
                    <p>Hemos recibido tu solicitud de compra de {order.Photos.Count} fotografía(s).</p>
                    
                    <h3>Resumen del Pedido:</h3>
                    <ul>
                        <li><strong>Número de Pedido:</strong> {order.Id.Substring(0, 8).ToUpper()}</li>
                        <li><strong>Cantidad de Fotos:</strong> {order.Photos.Count}</li>
                        {discountSection}
                        <li><strong>Total a Pagar:</strong> {order.Currency} {order.TotalAmount:F2}</li>
                        <li><strong>Fecha:</strong> {order.CreatedAt:dd/MM/yyyy HH:mm}</li>
                    </ul>

                    <h3>Estado: En Espera de Pago</h3>
                    <p style='background-color: #fff3cd; padding: 15px; border-left: 4px solid #ffc107; margin: 20px 0;'>
                        <strong>⏳ Tu orden será procesada una vez que el pago sea realizado y verificado.</strong>
                    </p>

                    <h3>Próximos Pasos:</h3>
                    <ol>
                        <li>Realiza la transferencia electrónica por el monto de <strong>{order.Currency} {order.TotalAmount:F2}</strong></li>
                        <li>Envía el comprobante de pago indicando tu número de pedido: <strong>{order.Id.Substring(0, 8).ToUpper()}</strong></li>
                        <li>Una vez confirmado el pago, procesaremos tus fotografías</li>
                        <li>Recibirás un correo de confirmación cuando tu orden esté lista</li>
                    </ol>

                    <p style='color: #666; font-size: 12px; margin-top: 30px;'>
                        <em>Una vez verificado tu pago, procesaremos tu orden y recibirás un correo electrónico cuando esté completa.</em>
                    </p>

                    <p>Si tienes alguna pregunta, no dudes en contactarnos.</p>
                    
                    <p>Saludos,<br>
                    <strong>{_settings.SenderName}</strong></p>
                </body>
                </html>";

        var message = new EmailMessage();
        message.From = _settings.SenderEmail;
        message.To.Add(userEmail);
        message.Subject = $"[Photos Market] - Orden Recibida - En Espera de Pago #{order.Id.Substring(0, 8)}";
        message.HtmlBody = htmlBody;

        try
        {
            await _resend.EmailSendAsync(message);
            _logger.LogInformation("Awaiting payment email sent successfully for order {OrderId}", order.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send awaiting payment email for order {OrderId}", order.Id);
            throw;
        }
    }

    public async Task SendPaymentConfirmedEmailAsync(Order order, string userEmail)
    {
        if (!_settings.Enabled)
        {
            _logger.LogInformation("Email service is disabled. Skipping payment confirmed email for order {OrderId}", order.Id);
            return;
        }

        // Construir sección de descuento si aplica
        var discountSection = "";
        if (order.DiscountPercentage.HasValue && order.DiscountPercentage.Value > 0)
        {
            discountSection = $@"
                        <li><strong>Subtotal:</strong> {order.Currency} {order.Subtotal:F2}</li>
                        <li style='color: green;'><strong>Descuento ({order.DiscountPercentage.Value}%):</strong> -{order.Currency} {order.DiscountAmount:F2}</li>";
        }

        var htmlBody = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>✅ ¡Pago Confirmado!</h2>
                    <p>Hemos recibido y validado tu pago exitosamente.</p>
                    
                    <div style='background-color: #d4edda; border-left: 4px solid #28a745; padding: 15px; margin: 20px 0;'>
                        <strong>✓ Tu pago ha sido verificado</strong><br>
                        <span style='color: #155724;'>Número de Pedido: {order.Id.Substring(0, 8).ToUpper()}</span>
                    </div>

                    <h3>Resumen del Pedido:</h3>
                    <ul>
                        <li><strong>Cantidad de Fotos:</strong> {order.Photos.Count}</li>
                        {discountSection}
                        <li><strong>Total Pagado:</strong> {order.Currency} {order.TotalAmount:F2}</li>
                        <li><strong>Fecha de Pago:</strong> {order.PaidAt?.ToString("dd/MM/yyyy HH:mm") ?? DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm")}</li>
                        {(string.IsNullOrEmpty(order.PaymentReference) ? "" : $"<li><strong>Referencia de Pago:</strong> {order.PaymentReference}</li>")}
                    </ul>

                    <h3>Próximos Pasos:</h3>
                    <div style='background-color: #cfe2ff; border-left: 4px solid #0d6efd; padding: 15px; margin: 20px 0;'>
                        <p style='margin: 0;'>
                            <strong>📸 Estamos preparando tus fotografías</strong><br>
                            Recibirás un nuevo correo electrónico con el enlace de descarga cuando tu orden esté completa.
                            Este proceso puede tomar algunos minutos.
                        </p>
                    </div>

                    <p style='color: #666; font-size: 12px; margin-top: 30px;'>
                        <em>⏱️ Por favor, espera el correo de confirmación de orden completa con el enlace de descarga. No cierres esta pestaña.</em>
                    </p>

                    <p>Si tienes alguna pregunta, no dudes en contactarnos.</p>
                    
                    <p>Saludos,<br>
                    <strong>{_settings.SenderName}</strong></p>
                </body>
                </html>";

        var message = new EmailMessage();
        message.From = _settings.SenderEmail;
        message.To.Add(userEmail);
        message.Subject = $"[Photos Market] - Pago Confirmado ✅ - Pedido #{order.Id.Substring(0, 8)}";
        message.HtmlBody = htmlBody;

        try
        {
            await _resend.EmailSendAsync(message);
            _logger.LogInformation("Payment confirmed email sent successfully for order {OrderId}", order.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send payment confirmed email for order {OrderId}", order.Id);
            throw;
        }
    }

    public async Task SendOrderCompletedEmailAsync(Order order, string userEmail)
    {
        if (!_settings.Enabled)
        {
            _logger.LogInformation("Email service is disabled. Skipping completed email for order {OrderId}", order.Id);
            return;
        }

        var htmlBody = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>¡Tu orden ha sido completada! ✅</h2>
                    <p>Nos complace informarte que tu pedido de {order.Photos.Count} fotografía(s) ha sido procesado exitosamente.</p>
                    
                    <h3>Resumen del Pedido:</h3>
                    <ul>
                        <li><strong>Número de Pedido:</strong> {order.Id.Substring(0, 8).ToUpper()}</li>
                        <li><strong>Cantidad de Fotos:</strong> {order.Photos.Count}</li>
                        <li><strong>Total Pagado:</strong> {order.Currency} {order.TotalAmount:F2}</li>
                        <li><strong>Fecha de Pago:</strong> {(order.PaidAt.HasValue ? order.PaidAt.Value.ToString("dd/MM/yyyy HH:mm") : "N/A")}</li>
                        <li><strong>Fecha de Procesamiento:</strong> {(order.ProcessedAt.HasValue ? order.ProcessedAt.Value.ToString("dd/MM/yyyy HH:mm") : "N/A")}</li>
                    </ul>

                    <div style='background-color: #d4edda; padding: 20px; margin: 20px 0; border-left: 4px solid #28a745; border-radius: 4px;'>
                        <h3 style='margin-top: 0; color: #155724;'>✅ Orden Completada</h3>
                        <p style='margin-bottom: 0; color: #155724;'>
                            Tu pedido ha sido completado exitosamente. Gracias por tu compra.
                        </p>
                    </div>

                    <p style='color: #666; font-size: 14px;'>
                        Tus fotografías han sido procesadas y enviadas a tu correo electrónico registrado. Revisa tu bandeja de entrada (y la carpeta de spam por si acaso) para acceder a tus archivos.
                    </p>

                    <p>Si tienes alguna pregunta o necesitas asistencia, no dudes en contactarnos.</p>
                    
                    <p>Gracias por tu confianza,<br>
                    <strong>{_settings.SenderName}</strong></p>
                </body>
                </html>";

        var message = new EmailMessage();
        message.From = _settings.SenderEmail;
        message.To.Add(userEmail);
        message.Subject = $"[Photos Market] - Orden Completada #{order.Id.Substring(0, 8)} ✅";
        message.HtmlBody = htmlBody;

        try
        {
            await _resend.EmailSendAsync(message);
            _logger.LogInformation("[Photos Market] - Completed email sent successfully for order {OrderId}", order.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send completed email for order {OrderId}", order.Id);
            throw;
        }
    }

    public async Task SendPaymentConfirmedWithDownloadLinkAsync(Order order, DownloadLink downloadLink, string userEmail)
    {
        if (!_settings.Enabled)
        {
            _logger.LogInformation("Email service is disabled. Skipping payment confirmed email for order {OrderId}", order.Id);
            return;
        }

        var downloadUrl = $"{_appSettings.FrontendUrl}/download/{downloadLink.Token}";
        var expirationDate = downloadLink.ExpiresAt;
        var hoursUntilExpiration = (int)(expirationDate - DateTime.UtcNow).TotalHours;

        // Construir sección de descuento si aplica
        var discountSection = "";
        if (order.DiscountPercentage.HasValue && order.DiscountPercentage.Value > 0)
        {
            discountSection = $@"
                        <li><strong>Subtotal:</strong> {order.Currency} {order.Subtotal:F2}</li>
                        <li style='color: green;'><strong>Descuento ({order.DiscountPercentage.Value}%):</strong> -{order.Currency} {order.DiscountAmount:F2}</li>";
        }

        var htmlBody = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>¡Pago Confirmado! 🎉</h2>
                    <p>Nos complace informarte que hemos recibido y confirmado tu pago por {order.Photos.Count} fotografía(s).</p>
                    
                    <h3>Resumen del Pedido:</h3>
                    <ul>
                        <li><strong>Número de Pedido:</strong> {order.Id.Substring(0, 8).ToUpper()}</li>
                        <li><strong>Cantidad de Fotos:</strong> {order.Photos.Count}</li>
                        {discountSection}
                        <li><strong>Total Pagado:</strong> {order.Currency} {order.TotalAmount:F2}</li>
                        <li><strong>Fecha de Pago:</strong> {(order.PaidAt.HasValue ? order.PaidAt.Value.ToString("dd/MM/yyyy HH:mm") : DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm"))}</li>
                    </ul>

                    <div style='background-color: #d4edda; padding: 20px; margin: 20px 0; border-left: 4px solid #28a745; border-radius: 4px;'>
                        <h3 style='margin-top: 0; color: #155724;'>✅ Pago Confirmado</h3>
                        <p style='margin-bottom: 0; color: #155724;'>
                            Tu pago ha sido verificado exitosamente. Ya puedes descargar tus fotografías en alta resolución.
                        </p>
                    </div>

                    <div style='background-color: #cfe2ff; padding: 20px; margin: 20px 0; border-left: 4px solid #0d6efd; border-radius: 4px;'>
                        <h3 style='margin-top: 0; color: #084298;'>📥 Descargar tus Fotografías</h3>
                        <p style='color: #084298;'>
                            Haz clic en el siguiente enlace para acceder a tus fotos en alta resolución (sin marca de agua):
                        </p>
                        <p style='text-align: center; margin: 20px 0;'>
                            <a href='{downloadUrl}' 
                               style='background-color: #0d6efd; color: white; padding: 12px 30px; text-decoration: none; 
                                      border-radius: 5px; font-weight: bold; display: inline-block;'>
                                📸 Descargar Mis Fotos
                            </a>
                        </p>
                        <p style='color: #084298; font-size: 14px;'>
                            O copia y pega este enlace en tu navegador:<br>
                            <code style='background-color: #e7f1ff; padding: 5px 10px; border-radius: 3px; display: inline-block; margin-top: 5px;'>{downloadUrl}</code>
                        </p>
                    </div>

                    <div style='background-color: #fff3cd; padding: 15px; border-left: 4px solid #ffc107; margin: 20px 0; border-radius: 4px;'>
                        <p style='margin: 0; color: #664d03;'>
                            <strong>⚠️ Importante:</strong> Este enlace estará disponible por <strong>{hoursUntilExpiration} horas</strong> 
                            (hasta el {expirationDate:dd/MM/yyyy HH:mm}).
                            Asegúrate de descargar todas tus fotos antes de que expire.
                        </p>
                    </div>

                    <h3>Instrucciones de Descarga:</h3>
                    <ol>
                        <li>Haz clic en el botón <strong>Descargar Mis Fotos</strong> de arriba</li>
                        <li>Serás redirigido a la página de descarga</li>
                        <li>Podrás descargar todas tus fotos en resolución original</li>
                        <li>Las fotos <strong>NO tendrán marca de agua</strong></li>
                        <li>Descarga todas las fotos antes de que expire el enlace</li>
                    </ol>

                    <p style='color: #666; font-size: 12px; margin-top: 30px;'>
                        <em>Si tienes problemas para acceder al enlace o necesitas una extensión del tiempo de descarga, 
                        no dudes en contactarnos.</em>
                    </p>

                    <p>¡Gracias por tu compra!<br>
                    <strong>{_settings.SenderName}</strong></p>
                </body>
                </html>";

        var message = new EmailMessage();
        message.From = _settings.SenderEmail;
        message.To.Add(userEmail);
        message.Subject = $"[Photos Market] - Pago Confirmado ✅ - Descarga tus Fotos #{order.Id.Substring(0, 8)}";
        message.HtmlBody = htmlBody;

        try
        {
            await _resend.EmailSendAsync(message);
            _logger.LogInformation("Payment confirmed email with download link sent successfully for order {OrderId}", order.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send payment confirmed email for order {OrderId}", order.Id);
            throw;
        }
    }
}
