using System.Threading.Tasks;

namespace LibraryAPI.Application.Interfaces
{
    public interface IEmailService
    {
        /// <summary>
        /// Envía un correo electrónico de forma asíncrona.
        /// </summary>
        /// <param name="to">Dirección de correo del destinatario.</param>
        /// <param name="subject">Asunto del correo.</param>
        /// <param name="body">Cuerpo del mensaje (soporta HTML).</param>
        /// <param name="isHtml">Indica si el cuerpo es HTML o texto plano.</param>
        /// <returns>Tarea que representa la operación asíncrona.</returns>
        Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
    }
}
