using Microsoft.AspNetCore.Identity;

namespace LibraryAPI.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public string FullName
        {
            get
            {
                var name = $"{FirstName} {LastName}".Trim();
                return string.IsNullOrWhiteSpace(name) ? Email ?? UserName ?? "Desconocido" : name;
            }
        }
    }
}
