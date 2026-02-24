using FluentValidation;
using LibraryAPI.Application.DTOs;

namespace LibraryAPI.Application.Validators
{
    public class CategoryCreateValidator : AbstractValidator<CategoryCreateDto>
    {
        public CategoryCreateValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Category name is required")
                .MaximumLength(100).WithMessage("Category name must not exceed 100 characters");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description must not exceed 500 characters");
        }
    }
}
