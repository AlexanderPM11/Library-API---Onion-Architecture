using LibraryAPI.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LibraryAPI.Application.Interfaces
{
    public interface IBookService
    {
        Task<ApiResponse<IEnumerable<BookDto>>> GetAllBooksAsync(int page, int pageSize);
        Task<ApiResponse<BookDto>> GetBookByIdAsync(int id);
        Task<ApiResponse<IEnumerable<BookDto>>> GetBooksByCategoryAsync(int categoryId);
        Task<ApiResponse<BookDto>> CreateBookAsync(BookCreateDto bookDto);
        Task<ApiResponse<bool>> UpdateBookAsync(int id, BookCreateDto bookDto);
        Task<ApiResponse<bool>> DeleteBookAsync(int id);
    }
}
