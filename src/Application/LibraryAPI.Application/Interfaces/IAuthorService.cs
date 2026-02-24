using LibraryAPI.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LibraryAPI.Application.Interfaces
{
    public interface IAuthorService
    {
        Task<ApiResponse<IEnumerable<AuthorDto>>> GetAllAuthorsAsync();
        Task<ApiResponse<AuthorDto>> GetAuthorByIdAsync(int id);
        Task<ApiResponse<AuthorDto>> CreateAuthorAsync(AuthorCreateDto authorDto);
        Task<ApiResponse<bool>> UpdateAuthorAsync(int id, AuthorCreateDto authorDto);
        Task<ApiResponse<bool>> DeleteAuthorAsync(int id);
    }
}
