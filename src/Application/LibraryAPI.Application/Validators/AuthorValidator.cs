using FluentValidation;
using LibraryAPI.Application.DTOs;

namespace LibraryAPI.Application.Validators
{
    public class AuthorCreateValidator : AbstractValidator<AuthorCreateDto>
    {
        public AuthorCreateValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");

            RuleFor(x => x.Biography)
                .MaximumLength(1000).WithMessage("Biography must not exceed 1000 characters");
        }
    }
}
