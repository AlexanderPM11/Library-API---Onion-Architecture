using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Application.DTOs
{
    public class ConfirmEmailDto
    {
        [Required(ErrorMessage = "El correo electrónico es requerido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El token de confirmación es requerido.")]
        public string Token { get; set; } = string.Empty;
    }
}
