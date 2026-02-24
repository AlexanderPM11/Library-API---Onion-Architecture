using System.Collections.Generic;

namespace LibraryAPI.Application.DTOs
{
    public class BookDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Isbn { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int PublicationYear { get; set; }
        public int Stock { get; set; }

        public CategoryDto Category { get; set; } = null!;
        public List<AuthorDto> Authors { get; set; } = new List<AuthorDto>();
    }

    public class BookCreateDto
    {
        public string Title { get; set; } = string.Empty;
        public string Isbn { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int PublicationYear { get; set; }
        public int Stock { get; set; }
        public int CategoryId { get; set; }
        public List<int> AuthorIds { get; set; } = new List<int>();
    }
}
