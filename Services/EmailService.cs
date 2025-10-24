using MailKit.Net.Smtp;
using MimeKit;
using System.Text;

namespace crud_park_back.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");
                var smtpServer = emailSettings["SmtpServer"];
                var smtpPort = int.Parse(emailSettings["SmtpPort"] ?? "587");
                var smtpUsername = emailSettings["SmtpUsername"];
                var smtpPassword = emailSettings["SmtpPassword"];
                var fromEmail = emailSettings["FromEmail"];
                var fromName = emailSettings["FromName"];

                // Si no hay configuración de email, retornar false sin error
                if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(smtpUsername) || 
                    string.IsNullOrEmpty(smtpPassword) || string.IsNullOrEmpty(fromEmail))
                {
                    _logger.LogWarning("Configuración de email no disponible. El envío de correos está deshabilitado.");
                    return false;
                }

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(fromName, fromEmail));
                message.To.Add(new MailboxAddress("", to));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder();
                if (isHtml)
                {
                    bodyBuilder.HtmlBody = body;
                }
                else
                {
                    bodyBuilder.TextBody = body;
                }
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(smtpServer, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(smtpUsername, smtpPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation($"Email enviado exitosamente a {to}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error enviando email a {to}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendMensualidadVencimientoNotificationAsync(string to, string nombre, string placa, DateTime fechaVencimiento)
        {
            var diasRestantes = (fechaVencimiento - DateTime.Now).Days;
            var subject = $"Recordatorio: Mensualidad próxima a vencer - Placa {placa}";
            
            var body = new StringBuilder();
            body.AppendLine("<html>");
            body.AppendLine("<body style='font-family: Arial, sans-serif;'>");
            body.AppendLine("<h2 style='color: #d32f2f;'>Recordatorio de Vencimiento de Mensualidad</h2>");
            body.AppendLine($"<p>Estimado/a <strong>{nombre}</strong>,</p>");
            body.AppendLine($"<p>Le informamos que su mensualidad para la placa <strong>{placa}</strong> vence en <strong>{diasRestantes} días</strong>.</p>");
            body.AppendLine($"<p><strong>Fecha de vencimiento:</strong> {fechaVencimiento:dd/MM/yyyy}</p>");
            body.AppendLine("<p>Para continuar disfrutando del servicio, le recomendamos renovar su mensualidad antes de la fecha de vencimiento.</p>");
            body.AppendLine("<p>Si ya renovó su mensualidad, puede ignorar este mensaje.</p>");
            body.AppendLine("<hr>");
            body.AppendLine("<p style='color: #666; font-size: 12px;'>Este es un mensaje automático del Sistema de Parqueadero.</p>");
            body.AppendLine("</body>");
            body.AppendLine("</html>");

            return await SendEmailAsync(to, subject, body.ToString());
        }

        public async Task<bool> SendMensualidadCreadaNotificationAsync(string to, string nombre, string placa, DateTime fechaInicio, DateTime fechaFin)
        {
            var subject = $"Confirmación: Mensualidad creada - Placa {placa}";
            
            var body = new StringBuilder();
            body.AppendLine("<html>");
            body.AppendLine("<body style='font-family: Arial, sans-serif;'>");
            body.AppendLine("<h2 style='color: #2e7d32;'>Confirmación de Mensualidad</h2>");
            body.AppendLine($"<p>Estimado/a <strong>{nombre}</strong>,</p>");
            body.AppendLine($"<p>Su mensualidad para la placa <strong>{placa}</strong> ha sido creada exitosamente.</p>");
            body.AppendLine($"<p><strong>Fecha de inicio:</strong> {fechaInicio:dd/MM/yyyy}</p>");
            body.AppendLine($"<p><strong>Fecha de vencimiento:</strong> {fechaFin:dd/MM/yyyy}</p>");
            body.AppendLine("<p>Ya puede utilizar el parqueadero sin restricciones durante el período de su mensualidad.</p>");
            body.AppendLine("<p>Le enviaremos un recordatorio antes del vencimiento para que pueda renovar si lo desea.</p>");
            body.AppendLine("<hr>");
            body.AppendLine("<p style='color: #666; font-size: 12px;'>Este es un mensaje automático del Sistema de Parqueadero.</p>");
            body.AppendLine("</body>");
            body.AppendLine("</html>");

            return await SendEmailAsync(to, subject, body.ToString());
        }
    }
}
