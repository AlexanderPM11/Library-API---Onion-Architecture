using AutoMapper;
using LibraryAPI.Application.DTOs;
using LibraryAPI.Application.Interfaces;
using LibraryAPI.Domain.Entities;
using LibraryAPI.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LibraryAPI.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CategoryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ApiResponse<IEnumerable<CategoryDto>>> GetAllCategoriesAsync()
        {
            var categories = await _unitOfWork.Categories.GetAllAsync();
            var categoriesDto = _mapper.Map<IEnumerable<CategoryDto>>(categories);
            return ApiResponse<IEnumerable<CategoryDto>>.SuccessResponse(categoriesDto);
        }

        public async Task<ApiResponse<CategoryDto>> GetCategoryByIdAsync(int id)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category == null) return ApiResponse<CategoryDto>.FailureResponse("Category not found");

            var categoryDto = _mapper.Map<CategoryDto>(category);
            return ApiResponse<CategoryDto>.SuccessResponse(categoryDto);
        }

        public async Task<ApiResponse<CategoryDto>> CreateCategoryAsync(CategoryCreateDto categoryDto)
        {
            var category = _mapper.Map<Category>(categoryDto);
            await _unitOfWork.Categories.AddAsync(category);
            await _unitOfWork.CompleteAsync();

            var result = _mapper.Map<CategoryDto>(category);
            return ApiResponse<CategoryDto>.SuccessResponse(result);
        }

        public async Task<ApiResponse<bool>> UpdateCategoryAsync(int id, CategoryCreateDto categoryDto)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category == null) return ApiResponse<bool>.FailureResponse("Category not found");

            _mapper.Map(categoryDto, category);
            category.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Categories.Update(category);
            await _unitOfWork.CompleteAsync();

            return ApiResponse<bool>.SuccessResponse(true, "Category updated successfully");
        }

        public async Task<ApiResponse<bool>> DeleteCategoryAsync(int id)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category == null) return ApiResponse<bool>.FailureResponse("Category not found");

            _unitOfWork.Categories.Remove(category);
            await _unitOfWork.CompleteAsync();

            return ApiResponse<bool>.SuccessResponse(true, "Category deleted successfully");
        }
    }
}
