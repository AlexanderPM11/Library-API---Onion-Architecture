using LibraryAPI.Domain.Entities;
using LibraryAPI.Domain.Interfaces;
using LibraryAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryAPI.Infrastructure.Repositories
{
    public class BookRepository : GenericRepository<Book>, IBookRepository
    {
        public BookRepository(LibraryDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Book>> GetBooksByCategoryAsync(int categoryId)
        {
            return await _context.Books
                .Include(b => b.Category)
                .Include(b => b.BookAuthors)
                    .ThenInclude(ba => ba.Author)
                .Where(b => b.CategoryId == categoryId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Book>> GetBooksWithDetailsAsync(int page, int pageSize)
        {
            return await _context.Books
                .Include(b => b.Category)
                .Include(b => b.BookAuthors)
                    .ThenInclude(ba => ba.Author)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Book?> GetBookWithDetailsAsync(int id)
        {
            return await _context.Books
                .Include(b => b.Category)
                .Include(b => b.BookAuthors)
                    .ThenInclude(ba => ba.Author)
                .FirstOrDefaultAsync(b => b.Id == id);
        }
    }
}
