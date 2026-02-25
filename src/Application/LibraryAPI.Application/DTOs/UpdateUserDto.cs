using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Application.DTOs
{
    public class UpdateUserDto
    {
        [Required(ErrorMessage = "First Name is required")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last Name is required")]
        public string LastName { get; set; } = string.Empty;

        public string? Email { get; set; }

        // Optional password change
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        public string? Password { get; set; }

        public string? Role { get; set; }

        public bool? IsActive { get; set; }
        public int? BranchId { get; set; }
    }
}
