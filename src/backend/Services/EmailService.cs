using Microsoft.Extensions.Options;
using PhotosMarket.API.Configuration;
using PhotosMarket.API.Models;
using Resend;

namespace PhotosMarket.API.Services;

public interface IEmailService
{
    Task SendOrderAwaitingPaymentEmailAsync(Order order, string userEmail);
    Task SendOrderCompletedEmailAsync(Order order, string userEmail);
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
                        Tus fotografías han sido procesadas. Para acceder a ellas, 
                        dirígete a la sección de órdenes en tu cuenta.
                    </p>

                    <p>Si tienes alguna pregunta o necesitas asistencia, no dudes en contactarnos.</p>
                    
                    <p>Gracias por tu confianza,<br>
                    <strong>{_settings.SenderName}</strong></p>
                </body>
                </html>";

        var message = new EmailMessage();
        message.From = _settings.SenderEmail;
        message.To.Add(userEmail);
        message.Subject = $"Orden Completada #{order.Id.Substring(0, 8)} ✅";
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
}
