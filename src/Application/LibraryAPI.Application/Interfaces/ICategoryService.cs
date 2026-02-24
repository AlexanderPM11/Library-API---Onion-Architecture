using LibraryAPI.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LibraryAPI.Application.Interfaces
{
    public interface ICategoryService
    {
        Task<ApiResponse<IEnumerable<CategoryDto>>> GetAllCategoriesAsync();
        Task<ApiResponse<CategoryDto>> GetCategoryByIdAsync(int id);
        Task<ApiResponse<CategoryDto>> CreateCategoryAsync(CategoryCreateDto categoryDto);
        Task<ApiResponse<bool>> UpdateCategoryAsync(int id, CategoryCreateDto categoryDto);
        Task<ApiResponse<bool>> DeleteCategoryAsync(int id);
    }
}
