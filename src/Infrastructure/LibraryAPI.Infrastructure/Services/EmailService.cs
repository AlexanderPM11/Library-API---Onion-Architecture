using LibraryAPI.Application.Interfaces;
using LibraryAPI.Domain.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Threading.Tasks;

namespace LibraryAPI.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
                email.To.Add(MailboxAddress.Parse(to));
                email.Subject = subject;

                var builder = new BodyBuilder();
                if (isHtml)
                {
                    builder.HtmlBody = body;
                }
                else
                {
                    builder.TextBody = body;
                }

                email.Body = builder.ToMessageBody();

                using var smtp = new SmtpClient();
                
                // Conectar al servidor SMTP
                await smtp.ConnectAsync(
                    _emailSettings.SmtpServer, 
                    _emailSettings.Port, 
                    _emailSettings.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls
                );

                // Autenticar si hay un usuario configurado
                if (!string.IsNullOrEmpty(_emailSettings.Username))
                {
                    await smtp.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
                }

                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

                _logger.LogInformation("Correo enviado exitosamente a {To} con el asunto: {Subject}", to, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar correo electrónico a {To}", to);
                throw new Exception("No se pudo enviar el correo electrónico. Por favor, intente más tarde.", ex);
            }
        }
    }
}
