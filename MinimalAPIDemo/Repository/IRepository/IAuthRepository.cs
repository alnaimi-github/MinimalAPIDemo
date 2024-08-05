using MinimalAPIDemo.Models.AuthDto;

namespace MinimalAPIDemo.Repository.IRepository
{
    public interface IAuthRepository
    {
        bool IsUniqueUser(string email);
        Task<TokenDto> LoginAsync(LoginRequestDto loginRequestDto);
        Task<UserDto> RegisterAsync(RegistrationRequestDto registrationRequestDto);
        Task<TokenDto> RefreshAccessToken(TokenDto tokenDto);
        Task RevokeRefreshToken(TokenDto tokenDto);
    }
}
