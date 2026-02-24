using LibraryAPI.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LibraryAPI.Domain.Interfaces
{
    public interface IBookRepository : IGenericRepository<Book>
    {
        Task<IEnumerable<Book>> GetBooksByCategoryAsync(int categoryId);
        Task<IEnumerable<Book>> GetBooksWithDetailsAsync(int page, int pageSize);
        Task<Book?> GetBookWithDetailsAsync(int id);
    }
}
