namespace LibraryAPI.Domain.Entities
{
    public class BookAuthor
    {
        public int BookId { get; set; }
        public virtual Book Book { get; set; } = null!;

        public int AuthorId { get; set; }
        public virtual Author Author { get; set; } = null!;
    }
}
