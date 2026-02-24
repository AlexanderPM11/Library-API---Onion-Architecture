using System.Collections.Generic;

namespace LibraryAPI.Domain.Entities
{
    public class Author : BaseEntity
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Biography { get; set; } = string.Empty;

        // Navigation properties
        public virtual ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();
    }
}
