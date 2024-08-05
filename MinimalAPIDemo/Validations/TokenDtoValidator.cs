using FluentValidation;
using MinimalAPIDemo.Models.AuthDto;

namespace MinimalAPIDemo.Validations
{
    public class TokenDtoValidator : AbstractValidator<TokenDto>
    {
        public TokenDtoValidator()
        {
            RuleFor(x => x.AccessToken).NotEmpty().WithMessage("Access Token is required.");
            RuleFor(x => x.RefreshToken).NotEmpty().WithMessage("Refresh Token is required.");
        }
    }
}
