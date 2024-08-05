using FluentValidation;
using MinimalAPIDemo.Models.AuthDto;

namespace MinimalAPIDemo.Validations
{
    public class LoginRequestDtoValidator : AbstractValidator<LoginRequestDto>
    {
        public LoginRequestDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.").EmailAddress();
            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("Email is required.")
                .MaximumLength(50).WithMessage("Username cannot be longer than 50 characters.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters long.");
        }
    }
}
