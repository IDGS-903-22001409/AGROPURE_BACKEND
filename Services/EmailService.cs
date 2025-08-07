using AGROPURE.Models.Enums;
using AGROPURE.Models.DTOs;
using System.Net.Mail;
using System.Net;

namespace AGROPURE.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendQuoteNotificationToAdminAsync(QuoteDto quote)
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

            await SendEmailAsync(adminEmail, subject, body);
        }

        public async Task SendQuoteRequestNotificationAsync(string customerEmail, int quoteId)
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

            await SendEmailAsync(customerEmail, subject, body);
        }

        public async Task SendQuoteStatusUpdateAsync(string customerEmail, int quoteId, QuoteStatus status)
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

            await SendEmailAsync(customerEmail, subject, body);
        }

        public async Task SendWelcomeEmailAsync(string email, string fullName, string tempPassword)
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

            await SendEmailAsync(email, subject, body);
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
                var senderEmail = _configuration["EmailSettings:SenderEmail"];
                var senderPassword = _configuration["EmailSettings:SenderPassword"];

                using var client = new SmtpClient(smtpServer, smtpPort)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(senderEmail, senderPassword)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, "AGROPURE Sistema"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enviando email a {toEmail}: {ex.Message}");
                throw;
            }
        }
    }
}
