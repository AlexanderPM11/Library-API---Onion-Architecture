using FluentValidation;
using LibraryAPI.Application.DTOs;
using System;

namespace LibraryAPI.Application.Validators
{
    public class BookCreateValidator : AbstractValidator<BookCreateDto>
    {
        public BookCreateValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Book title is required")
                .MaximumLength(200).WithMessage("Book title must not exceed 200 characters");

            RuleFor(x => x.Isbn)
                .NotEmpty().WithMessage("ISBN is required")
                .Length(10, 13).WithMessage("ISBN must be between 10 and 13 characters");

            RuleFor(x => x.PublicationYear)
                .InclusiveBetween(1000, DateTime.Now.Year).WithMessage("Invalid publication year");

            RuleFor(x => x.Stock)
                .GreaterThanOrEqualTo(0).WithMessage("Stock cannot be negative");

            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("Category ID is required");

            RuleFor(x => x.AuthorIds)
                .NotEmpty().WithMessage("At least one author is required");
        }
    }
}
