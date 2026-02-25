using AutoMapper;
using LibraryAPI.Application.DTOs;
using LibraryAPI.Domain.Entities;
using System.Linq;

namespace LibraryAPI.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Category Mappings
            CreateMap<Category, CategoryDto>().ReverseMap();
            CreateMap<CategoryCreateDto, Category>();

            // Author Mappings
            CreateMap<Author, AuthorDto>().ReverseMap();
            CreateMap<AuthorCreateDto, Author>();

            // Book Mappings
            CreateMap<Book, BookDto>()
                .ForMember(dest => dest.Authors, opt => opt.MapFrom(src => src.BookAuthors.Select(ba => ba.Author)));

            CreateMap<BookCreateDto, Book>()
                .ForMember(dest => dest.BookAuthors, opt => opt.Ignore()); // Will be handled manually in the service

            CreateMap<AuditLog, AuditLogDto>();
        }
    }
}
