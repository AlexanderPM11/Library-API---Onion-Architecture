using System;
using System.Collections.Generic;

namespace LibraryAPI.Domain.Entities
{
    public class Book : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Isbn { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int PublicationYear { get; set; }
        public int Stock { get; set; }

        public int? BranchId { get; set; }
        public virtual Branch? Branch { get; set; }

        public int CategoryId { get; set; }
        public virtual Category Category { get; set; } = null!;

        // Navigation properties
        public virtual ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();
    }
}
