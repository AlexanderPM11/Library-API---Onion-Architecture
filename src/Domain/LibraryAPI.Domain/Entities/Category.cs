using System.Collections.Generic;

namespace LibraryAPI.Domain.Entities
{
    public class Category : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Navigation properties
        public virtual ICollection<Book> Books { get; set; } = new List<Book>();
    }
}
