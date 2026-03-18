using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Options;
using PhotosMarket.API.Configuration;
using PhotosMarket.API.Models;

namespace PhotosMarket.API.Services;

public interface IEmailService
{
    Task SendOrderConfirmationEmailAsync(Order order, string userEmail);
    Task SendDownloadLinkEmailAsync(string userEmail, string downloadUrl, DateTime expiresAt);
}

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ApplicationSettings _appSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<EmailSettings> settings,
        IOptions<ApplicationSettings> appSettings,
        ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _appSettings = appSettings.Value;
        _logger = logger;
    }

    public async Task SendOrderConfirmationEmailAsync(Order order, string userEmail)
    {
        if (!_settings.Enabled)
        {
            _logger.LogInformation("Email service is disabled. Skipping order confirmation email for order {OrderId}", order.Id);
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
        message.To.Add(new MailboxAddress("", userEmail));
        message.Subject = $"Confirmación de Pedido #{order.Id.Substring(0, 8)}";

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>¡Gracias por tu pedido!</h2>
                    <p>Hemos recibido tu solicitud de compra de {order.Photos.Count} fotografía(s).</p>
                    
                    <h3>Resumen del Pedido:</h3>
                    <ul>
                        <li><strong>Número de Pedido:</strong> {order.Id.Substring(0, 8).ToUpper()}</li>
                        <li><strong>Cantidad de Fotos:</strong> {order.Photos.Count}</li>
                        <li><strong>Total:</strong> {order.Currency} {order.TotalAmount:F2}</li>
                        <li><strong>Fecha:</strong> {order.CreatedAt:dd/MM/yyyy HH:mm}</li>
                    </ul>

                    <h3>Próximos Pasos:</h3>
                    <ol>
                        <li>Realiza la transferencia electrónica por el monto de <strong>{order.Currency} {order.TotalAmount:F2}</strong></li>
                        <li>Envía el comprobante de pago indicando tu número de pedido: <strong>{order.Id.Substring(0, 8).ToUpper()}</strong></li>
                        <li>Una vez confirmado el pago, procesaremos tus fotografías</li>
                        <li>Recibirás un correo con el enlace de descarga de tus fotos en alta resolución</li>
                    </ol>

                    <p style='color: #666; font-size: 12px; margin-top: 30px;'>
                        <em>El enlace de descarga estará disponible por {_appSettings.DownloadLinkExpirationHours} horas después de recibir el pago.</em>
                    </p>

                    <p>Si tienes alguna pregunta, no dudes en contactarnos.</p>
                    
                    <p>Saludos,<br>
                    <strong>{_settings.SenderName}</strong></p>
                </body>
                </html>"
        };

        message.Body = bodyBuilder.ToMessageBody();

        await SendEmailAsync(message);
    }

    public async Task SendDownloadLinkEmailAsync(string userEmail, string downloadUrl, DateTime expiresAt)
    {
        if (!_settings.Enabled)
        {
            _logger.LogInformation("Email service is disabled. Skipping download link email for {UserEmail}", userEmail);
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
        message.To.Add(new MailboxAddress("", userEmail));
        message.Subject = "Tus Fotografías Están Listas para Descargar";

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>¡Tus fotografías están listas!</h2>
                    <p>Hemos procesado tu pedido y tus fotografías en alta resolución están disponibles para descarga.</p>
                    
                    <div style='background-color: #f5f5f5; padding: 20px; margin: 20px 0; border-radius: 5px;'>
                        <p style='margin: 0;'><strong>Enlace de Descarga:</strong></p>
                        <p style='margin: 10px 0;'>
                            <a href='{downloadUrl}' 
                               style='background-color: #4CAF50; color: white; padding: 12px 24px; 
                                      text-decoration: none; border-radius: 4px; display: inline-block;'>
                                Descargar Fotografías
                            </a>
                        </p>
                    </div>

                    <p style='color: #d32f2f; font-weight: bold;'>
                        ⚠️ IMPORTANTE: Este enlace expirará el {expiresAt:dd/MM/yyyy} a las {expiresAt:HH:mm} UTC
                    </p>

                    <p style='color: #666; font-size: 14px;'>
                        Asegúrate de descargar tus fotografías antes de la fecha de expiración. 
                        Después de esa fecha, el enlace dejará de funcionar.
                    </p>

                    <p>Si tienes algún problema con la descarga, contáctanos inmediatamente.</p>
                    
                    <p>Gracias por tu compra,<br>
                    <strong>{_settings.SenderName}</strong></p>
                </body>
                </html>"
        };

        message.Body = bodyBuilder.ToMessageBody();

        await SendEmailAsync(message);
    }

    private async Task SendEmailAsync(MimeMessage message)
    {
        using var client = new SmtpClient();
        
        await client.ConnectAsync(_settings.SmtpServer, _settings.SmtpPort, 
            _settings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
        
        await client.AuthenticateAsync(_settings.SenderEmail, _settings.SenderPassword);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
