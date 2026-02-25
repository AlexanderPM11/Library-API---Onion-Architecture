using LibraryAPI.Application.Interfaces;
using LibraryAPI.Domain.Settings;
using LibraryAPI.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryAPI.Infrastructure.DependencyInjection
{
    public static class InfrastructureServiceExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<EmailSettings>(options =>
            {
                var section = configuration.GetSection("EmailSettings");
                options.SmtpServer = Environment.GetEnvironmentVariable("SMTP_SERVER") ?? section["SmtpServer"] ?? string.Empty;
                options.Port = int.TryParse(Environment.GetEnvironmentVariable("SMTP_PORT"), out var port) ? port : section.GetValue<int>("Port");
                options.SenderName = Environment.GetEnvironmentVariable("SMTP_SENDER_NAME") ?? section["SenderName"] ?? string.Empty;
                options.SenderEmail = Environment.GetEnvironmentVariable("SMTP_SENDER_EMAIL") ?? section["SenderEmail"] ?? string.Empty;
                options.Username = Environment.GetEnvironmentVariable("SMTP_USERNAME") ?? section["Username"] ?? string.Empty;
                options.Password = Environment.GetEnvironmentVariable("SMTP_PASSWORD") ?? section["Password"] ?? string.Empty;
                options.UseSsl = bool.TryParse(Environment.GetEnvironmentVariable("SMTP_USE_SSL"), out var useSsl) ? useSsl : section.GetValue<bool>("UseSsl");
            });

            services.Configure<FrontendSettings>(options =>
            {
                var section = configuration.GetSection("FrontendSettings");
                options.BaseUrl = Environment.GetEnvironmentVariable("FRONTEND_BASE_URL") ?? section["BaseUrl"] ?? "http://localhost:5173";
                options.ConfirmEmailPath = Environment.GetEnvironmentVariable("FRONTEND_CONFIRM_EMAIL_PATH") ?? section["ConfirmEmailPath"] ?? "/confirm-email";
                options.ResetPasswordPath = Environment.GetEnvironmentVariable("FRONTEND_RESET_PASSWORD_PATH") ?? section["ResetPasswordPath"] ?? "/reset-password";
            });

            // Registro de Servicios
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IEmailTemplateService, EmailTemplateService>();

            return services;
        }
    }
}
