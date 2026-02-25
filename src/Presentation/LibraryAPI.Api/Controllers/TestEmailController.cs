using LibraryAPI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LibraryAPI.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestEmailController : ControllerBase
    {
        private readonly IEmailService _emailService;

        public TestEmailController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost("send")]
        [AllowAnonymous] // Para facilitar la prueba, considerá restringirlo en producción.
        public async Task<IActionResult> SendTestEmail([FromBody] TestEmailRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.To))
            {
                return BadRequest("El destinatario (To) es requerido.");
            }

            var subject = request.Subject ?? "Prueba de Integración - LibraryApp Email Service";
            
            var body = request.IsHtml 
                ? $"<h1>¡Hola!</h1><p>Este es un correo de prueba en HTML para validar el <strong>EmailService</strong> en LibraryApp.</p><p>Mensaje personalizado: {request.Body}</p>"
                : $"¡Hola!\n\nEste es un correo de prueba en texto plano para validar el EmailService en LibraryApp.\n\nMensaje personalizado: {request.Body}";

            try
            {
                await _emailService.SendEmailAsync(request.To, subject, body, request.IsHtml);
                return Ok(new { Message = "Correo de prueba enviado exitosamente a " + request.To });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { Message = "Error al enviar el correo.", Error = ex.Message, InnerException = ex.InnerException?.Message });
            }
        }
    }

    public class TestEmailRequest
    {
        public string To { get; set; } = string.Empty;
        public string? Subject { get; set; }
        public string? Body { get; set; }
        public bool IsHtml { get; set; } = true;
    }
}
