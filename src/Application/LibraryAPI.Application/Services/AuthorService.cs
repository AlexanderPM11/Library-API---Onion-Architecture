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
    public class AuthorService : IAuthorService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AuthorService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ApiResponse<IEnumerable<AuthorDto>>> GetAllAuthorsAsync()
        {
            var authors = await _unitOfWork.Authors.GetAllAsync();
            var authorsDto = _mapper.Map<IEnumerable<AuthorDto>>(authors);
            return ApiResponse<IEnumerable<AuthorDto>>.SuccessResponse(authorsDto);
        }

        public async Task<ApiResponse<AuthorDto>> GetAuthorByIdAsync(int id)
        {
            var author = await _unitOfWork.Authors.GetByIdAsync(id);
            if (author == null) return ApiResponse<AuthorDto>.FailureResponse("Author not found");

            var authorDto = _mapper.Map<AuthorDto>(author);
            return ApiResponse<AuthorDto>.SuccessResponse(authorDto);
        }

        public async Task<ApiResponse<AuthorDto>> CreateAuthorAsync(AuthorCreateDto authorDto)
        {
            var author = _mapper.Map<Author>(authorDto);
            await _unitOfWork.Authors.AddAsync(author);
            await _unitOfWork.CompleteAsync();

            var result = _mapper.Map<AuthorDto>(author);
            return ApiResponse<AuthorDto>.SuccessResponse(result);
        }

        public async Task<ApiResponse<bool>> UpdateAuthorAsync(int id, AuthorCreateDto authorDto)
        {
            var author = await _unitOfWork.Authors.GetByIdAsync(id);
            if (author == null) return ApiResponse<bool>.FailureResponse("Author not found");

            _mapper.Map(authorDto, author);
            author.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Authors.Update(author);
            await _unitOfWork.CompleteAsync();

            return ApiResponse<bool>.SuccessResponse(true, "Author updated successfully");
        }

        public async Task<ApiResponse<bool>> DeleteAuthorAsync(int id)
        {
            var author = await _unitOfWork.Authors.GetByIdAsync(id);
            if (author == null) return ApiResponse<bool>.FailureResponse("Author not found");

            _unitOfWork.Authors.Remove(author);
            await _unitOfWork.CompleteAsync();

            return ApiResponse<bool>.SuccessResponse(true, "Author deleted successfully");
        }
    }
}
