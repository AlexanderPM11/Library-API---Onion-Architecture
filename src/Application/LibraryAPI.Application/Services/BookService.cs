using AutoMapper;
using LibraryAPI.Application.DTOs;
using LibraryAPI.Application.Interfaces;
using LibraryAPI.Domain.Entities;
using LibraryAPI.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryAPI.Application.Services
{
    public class BookService : IBookService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public BookService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ApiResponse<IEnumerable<BookDto>>> GetAllBooksAsync(int page, int pageSize)
        {
            var books = await _unitOfWork.Books.GetBooksWithDetailsAsync(page, pageSize);
            var booksDto = _mapper.Map<IEnumerable<BookDto>>(books);
            return ApiResponse<IEnumerable<BookDto>>.SuccessResponse(booksDto);
        }

        public async Task<ApiResponse<BookDto>> GetBookByIdAsync(int id)
        {
            var book = await _unitOfWork.Books.GetBookWithDetailsAsync(id);
            if (book == null) return ApiResponse<BookDto>.FailureResponse("Book not found");

            var bookDto = _mapper.Map<BookDto>(book);
            return ApiResponse<BookDto>.SuccessResponse(bookDto);
        }

        public async Task<ApiResponse<IEnumerable<BookDto>>> GetBooksByCategoryAsync(int categoryId)
        {
            var books = await _unitOfWork.Books.GetBooksByCategoryAsync(categoryId);
            var booksDto = _mapper.Map<IEnumerable<BookDto>>(books);
            return ApiResponse<IEnumerable<BookDto>>.SuccessResponse(booksDto);
        }

        public async Task<ApiResponse<BookDto>> CreateBookAsync(BookCreateDto bookDto)
        {
            try
            {
                var book = _mapper.Map<Book>(bookDto);

                // Add Authors
                foreach (var authorId in bookDto.AuthorIds)
                {
                    book.BookAuthors.Add(new BookAuthor { AuthorId = authorId });
                }

                await _unitOfWork.Books.AddAsync(book);
                await _unitOfWork.CompleteAsync();

                var result = await GetBookByIdAsync(book.Id);
                return result;
            }
            catch (Exception ex)
            {
                return ApiResponse<BookDto>.FailureResponse($"Error creating book: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> UpdateBookAsync(int id, BookCreateDto bookDto)
        {
            var book = await _unitOfWork.Books.GetBookWithDetailsAsync(id);
            if (book == null) return ApiResponse<bool>.FailureResponse("Book not found");

            _mapper.Map(bookDto, book);
            book.UpdatedAt = DateTime.UtcNow;

            // Update Authors (Simple clear and add)
            book.BookAuthors.Clear();
            foreach (var authorId in bookDto.AuthorIds)
            {
                book.BookAuthors.Add(new BookAuthor { AuthorId = authorId, BookId = id });
            }

            _unitOfWork.Books.Update(book);
            await _unitOfWork.CompleteAsync();

            return ApiResponse<bool>.SuccessResponse(true, "Book updated successfully");
        }

        public async Task<ApiResponse<bool>> DeleteBookAsync(int id)
        {
            var book = await _unitOfWork.Books.GetByIdAsync(id);
            if (book == null) return ApiResponse<bool>.FailureResponse("Book not found");

            _unitOfWork.Books.Remove(book);
            await _unitOfWork.CompleteAsync();

            return ApiResponse<bool>.SuccessResponse(true, "Book deleted successfully");
        }
    }
}
