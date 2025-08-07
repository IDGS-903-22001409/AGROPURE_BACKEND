using AGROPURE.Models.Enums;
using AGROPURE.Models.DTOs;
using System.Net.Mail;
using System.Net;

namespace AGROPURE.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendQuoteNotificationToAdminAsync(QuoteDto quote)
        {
            try
            {
                var adminEmail = _configuration["EmailSettings:AdminEmail"] ?? "admin@agropure.com";
                var subject = $"Nueva Cotización #{quote.Id} - AGROPURE";
                var body = $@"
                    <h2>Nueva Solicitud de Cotización</h2>
                    <p><strong>ID:</strong> #{quote.Id}</p>
                    <p><strong>Cliente:</strong> {quote.CustomerName}</p>
                    <p><strong>Email:</strong> {quote.CustomerEmail}</p>
                    <p><strong>Teléfono:</strong> {quote.CustomerPhone}</p>
                    <p><strong>Empresa:</strong> {quote.CustomerCompany}</p>
                    <p><strong>Producto:</strong> {quote.ProductName}</p>
                    <p><strong>Cantidad:</strong> {quote.Quantity}</p>
                    <p><strong>Total:</strong> ${quote.TotalCost:F2}</p>
                    <p><strong>Notas:</strong> {quote.Notes}</p>
                    <p><strong>Fecha:</strong> {quote.RequestDate:dd/MM/yyyy HH:mm}</p>
                    <br>
                    <p>Ingresa al panel de administración para gestionar esta cotización.</p>
                    <p><a href='https://localhost:4200/admin/quotes'>Ver Cotizaciones</a></p>
                ";

                _logger.LogInformation("Enviando notificación de cotización #{QuoteId} a {AdminEmail}", quote.Id, adminEmail);
                await SendEmailAsync(adminEmail, subject, body);
                _logger.LogInformation("Notificación de cotización #{QuoteId} enviada exitosamente", quote.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando notificación de cotización #{QuoteId}", quote.Id);
                throw;
            }
        }

        public async Task SendQuoteRequestNotificationAsync(string customerEmail, int quoteId)
        {
            try
            {
                var subject = "Cotización Recibida - AGROPURE";
                var body = $@"
                    <h2>¡Gracias por tu interés en AGROPURE!</h2>
                    <p>Hemos recibido tu solicitud de cotización #{quoteId}.</p>
                    <p>Nuestro equipo la revisará y te enviaremos una respuesta en las próximas 24-48 horas.</p>
                    <p>Puedes consultar el estado de tu cotización en nuestro portal web.</p>
                    <br>
                    <p>Saludos cordiales,<br>
                    El equipo de AGROPURE</p>
                ";

                _logger.LogInformation("Enviando confirmación de cotización #{QuoteId} a {CustomerEmail}", quoteId, customerEmail);
                await SendEmailAsync(customerEmail, subject, body);
                _logger.LogInformation("Confirmación de cotización #{QuoteId} enviada exitosamente", quoteId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando confirmación de cotización #{QuoteId} a {CustomerEmail}", quoteId, customerEmail);
                // No lanzar excepción aquí para que no falle la creación de cotización
            }
        }

        public async Task SendQuoteStatusUpdateAsync(string customerEmail, int quoteId, QuoteStatus status)
        {
            try
            {
                var statusText = status switch
                {
                    QuoteStatus.Approved => "Aprobada",
                    QuoteStatus.Rejected => "Rechazada",
                    QuoteStatus.Completed => "Completada",
                    _ => "Actualizada"
                };

                var subject = $"Cotización #{quoteId} {statusText} - AGROPURE";
                var body = $@"
                    <h2>Estado de tu Cotización Actualizado</h2>
                    <p>Tu cotización #{quoteId} ha sido <strong>{statusText.ToLower()}</strong>.</p>
                    <p>Puedes ver los detalles completos en nuestro portal web.</p>
                    <br>
                    <p>Saludos cordiales,<br>
                    El equipo de AGROPURE</p>
                ";

                _logger.LogInformation("Enviando actualización de estado de cotización #{QuoteId} a {CustomerEmail}: {Status}",
                    quoteId, customerEmail, statusText);
                await SendEmailAsync(customerEmail, subject, body);
                _logger.LogInformation("Actualización de cotización #{QuoteId} enviada exitosamente", quoteId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando actualización de cotización #{QuoteId} a {CustomerEmail}",
                    quoteId, customerEmail);
            }
        }

        public async Task SendWelcomeEmailAsync(string email, string fullName, string tempPassword)
        {
            try
            {
                var subject = "Bienvenido a AGROPURE";
                var body = $@"
                    <h2>¡Bienvenido a AGROPURE, {fullName}!</h2>
                    <p>Tu cuenta ha sido creada exitosamente tras la aprobación de tu cotización.</p>
                    <p><strong>Email:</strong> {email}</p>
                    <p><strong>Contraseña temporal:</strong> {tempPassword}</p>
                    <p><strong>Importante:</strong> Por favor, cambia tu contraseña después del primer inicio de sesión por seguridad.</p>
                    <br>
                    <p>Ahora puedes:</p>
                    <ul>
                        <li>Acceder a tu cuenta en nuestro portal</li>
                        <li>Ver el historial de tus cotizaciones</li>
                        <li>Solicitar nuevas cotizaciones</li>
                        <li>Dejar reseñas de productos</li>
                    </ul>
                    <br>
                    <p><a href='https://localhost:4200/login'>Iniciar Sesión</a></p>
                    <br>
                    <p>Saludos cordiales,<br>
                    El equipo de AGROPURE</p>
                ";

                _logger.LogInformation("Enviando email de bienvenida a {Email}", email);
                await SendEmailAsync(email, subject, body);
                _logger.LogInformation("Email de bienvenida enviado exitosamente a {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando email de bienvenida a {Email}", email);
            }
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                // Obtener configuración de email
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPortString = _configuration["EmailSettings:SmtpPort"];
                var senderEmail = _configuration["EmailSettings:SenderEmail"];
                var senderPassword = _configuration["EmailSettings:SenderPassword"];
                var enableSslString = _configuration["EmailSettings:EnableSsl"];

                _logger.LogInformation("Configuración de email: Server={SmtpServer}, Port={SmtpPort}, From={SenderEmail}",
                    smtpServer, smtpPortString, senderEmail);

                // Validar configuración
                if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(senderEmail))
                {
                    _logger.LogWarning("Configuración de email incompleta. Email no enviado a {ToEmail}", toEmail);
                    return; // No enviar si no está configurado
                }

                if (!int.TryParse(smtpPortString, out int smtpPort))
                {
                    smtpPort = 587; // Puerto por defecto
                }

                if (!bool.TryParse(enableSslString, out bool enableSsl))
                {
                    enableSsl = true; // SSL por defecto
                }

                using var client = new SmtpClient(smtpServer, smtpPort)
                {
                    EnableSsl = enableSsl,
                    Credentials = new NetworkCredential(senderEmail, senderPassword),
                    Timeout = 30000 // 30 segundos timeout
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, "AGROPURE Sistema"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                _logger.LogInformation("Enviando email a {ToEmail} con asunto: {Subject}", toEmail, subject);
                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Email enviado exitosamente a {ToEmail}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando email a {ToEmail}: {ErrorMessage}", toEmail, ex.Message);
                throw; // Re-lanzar para que el caller sepa que falló
            }
        }
    }
}