using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MinimalAPIDemo.Data;
using MinimalAPIDemo.Models;
using MinimalAPIDemo.Models.AuthDto;
using MinimalAPIDemo.Repository.IRepository;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MinimalAPIDemo.Repository
{
    public class AuthRepository(ApplicationDbContext db,
        IConfiguration config,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<JwtOptions> jwtOptions) : IAuthRepository
    {
        #region IsUniqueUser

        public bool IsUniqueUser(string email)
        {
            var user = db.ApplicationUsers.FirstOrDefault(x
                => x.Email == email);
            if (user is null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region Login

        public async Task<TokenDto> LoginAsync(LoginRequestDto loginRequestDto)
        {
            var checkIsValidUserFromDb = await db.ApplicationUsers.FirstOrDefaultAsync(x
                => x.UserName!.ToLower() == loginRequestDto.UserName.ToLower());
            var checkIsValidPasswordFromDb = await userManager.CheckPasswordAsync(checkIsValidUserFromDb!, loginRequestDto.Password);
            if (checkIsValidUserFromDb == null || checkIsValidPasswordFromDb == false)
            {
                return new TokenDto() { AccessToken = "" };
            }
            var jwtTokenId = $"JTI{Guid.NewGuid()}";
            var accessToken = await GenerateAccessToken(checkIsValidUserFromDb, jwtTokenId);
            var refreshToken = await CreateNewRefreshToken(checkIsValidUserFromDb.Id, jwtTokenId);
            var loginResponseDto = new TokenDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
            return loginResponseDto;
        }

        #endregion

        #region Register

        public async Task<UserDto> RegisterAsync(RegistrationRequestDto registerRequestDto)
        {
            var user = new ApplicationUser()
            {
                Name = registerRequestDto.UserName,
                UserName = registerRequestDto.UserName,
                Email = registerRequestDto.Email,
                NormalizedEmail = registerRequestDto.UserName,
                NormalizedUserName = registerRequestDto.UserName,
            };
            try
            {
                var result = await userManager.CreateAsync(user, registerRequestDto.Password);
                if (result.Succeeded)
                {
                    if (!await roleManager.RoleExistsAsync(registerRequestDto.Role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(registerRequestDto.Role));
                    }

                    await userManager.AddToRoleAsync(user, registerRequestDto.Role);
                    var userToReturn =
                        await db.ApplicationUsers.
                            FirstOrDefaultAsync(x => x.Email == registerRequestDto.Email);
                    return new UserDto()
                    {
                        Id = userToReturn!.Id,
                        Email = userToReturn.Email!,
                        UserName = userToReturn.UserName!
                    };
                }
            }
            catch (Exception e)
            {
            }

            return new UserDto();
        }

        #endregion

        #region RefreshAccessToken

        public async Task<TokenDto> RefreshAccessToken(TokenDto tokenDto)
        {
            // Find an existing refresh token
            var existingRefreshToken = await ReturnExistingRefreshToken(tokenDto);
            if (existingRefreshToken == null)
            {
                return new TokenDto();
            }
            // Compare data from existing refresh and access token provided and if there is any mismatch then consider it as a 
            var isTokenValid = GetAccessTokenData(tokenDto.AccessToken, existingRefreshToken.UserId, existingRefreshToken.JwtTokenId);
            if (!isTokenValid)
            {
                await MarkTokenAsInValid(existingRefreshToken);
                return new TokenDto();
            }
            // when someone tries to use not valid refresh token , fraud possible
            if (!existingRefreshToken.IsValid)
            {
                await MarkAllTokenInChainAsInvalid(existingRefreshToken.UserId, existingRefreshToken.JwtTokenId);
                return new TokenDto();
            }

            // if just expired  then make  as invalid and return empty
            if (existingRefreshToken.ExpireAt < DateTime.UtcNow)
            {
                await MarkTokenAsInValid(existingRefreshToken);
                return new TokenDto();
            }

            // replace old refresh token with a new one with updated expire date
            var newRefreshToken =
                await CreateNewRefreshToken(existingRefreshToken.UserId, existingRefreshToken.JwtTokenId);

            // revoke existing  refresh token
            await MarkTokenAsInValid(existingRefreshToken);

            // generate new access token
            var applicationUser =
                await db.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == existingRefreshToken.UserId);
            if (applicationUser == null)
            {
                return new TokenDto();
            }

            var newAccessToken = await GenerateAccessToken(applicationUser, existingRefreshToken.JwtTokenId);
            return new TokenDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
            };
        }

        #region RevokeRefreshToken

        public async Task RevokeRefreshToken(TokenDto tokenDto)
        {
            var existingRefreshToken = await ReturnExistingRefreshToken(tokenDto);
            if (existingRefreshToken == null) return;

            var isTokenValid = GetAccessTokenData(tokenDto.AccessToken, existingRefreshToken.UserId, existingRefreshToken.JwtTokenId);
            if (!isTokenValid)
            {
                return;
            }
            await MarkAllTokenInChainAsInvalid(existingRefreshToken.UserId, existingRefreshToken.JwtTokenId);
        }

        #endregion

        #endregion

        #region GenerateAccessToken

        private async Task<string> GenerateAccessToken(ApplicationUser user, string tokenId)
        {

            var roles = await userManager.GetRolesAsync(user);
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(jwtOptions.Value.SecretKey);
            var claimList = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Email,user.Email!),
                new(JwtRegisteredClaimNames.Sub,user.Id!),
                new(JwtRegisteredClaimNames.Name,user.Name!),
                new(JwtRegisteredClaimNames.Jti,tokenId),
            };
            claimList.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claimList),
                Expires = DateTime.UtcNow.AddMinutes(1),
                Issuer = jwtOptions.Value.Issuer,
                Audience = jwtOptions.Value.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            return tokenString;
        }

        #endregion

        #region CreateNewRefreshToken

        private async Task<string> CreateNewRefreshToken(string userId, string tokenId)
        {
            var refreshToken = new RefreshToken
            {
                IsValid = true,
                UserId = userId,
                JwtTokenId = tokenId,
                ExpireAt = DateTime.UtcNow.AddMinutes(2),
                RefreshJwtToken = Guid.NewGuid() + "-" + Guid.NewGuid()
            };
            await db.RefreshTokens.AddAsync(refreshToken);
            await db.SaveChangesAsync();
            return refreshToken.RefreshJwtToken;
        }

        #endregion

        #region GetAccessTokenData

        private static bool GetAccessTokenData(string accessToken, string expectedUserId, string expectedTokenId)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwt = tokenHandler.ReadJwtToken(accessToken);
                var jwtTokenId = jwt.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Jti)!.Value;
                var userId = jwt.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Sub)!.Value;
                return userId == expectedUserId && jwtTokenId == expectedTokenId;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        #endregion

        #region MarkAllTokenInChainAsInvalid

        private async Task MarkAllTokenInChainAsInvalid(string userId, string tokenId)
        {
            await db.RefreshTokens.Where(u =>
                    u.UserId == userId
                    && u.JwtTokenId == tokenId)
                .ExecuteUpdateAsync(u => u.SetProperty(
                    refreshToken => refreshToken.IsValid, false));
        }

        #endregion

        #region MarkTokenAsInValid

        private async Task MarkTokenAsInValid(RefreshToken refreshToken)
        {
            refreshToken.IsValid = false;
            await db.SaveChangesAsync();
        }

        #endregion

        #region ReturnExistingRefreshToken

        private async Task<RefreshToken?> ReturnExistingRefreshToken(TokenDto tokenDto)
        {
            var existingRefreshToken =
                await db.RefreshTokens.FirstOrDefaultAsync(u => u.RefreshJwtToken == tokenDto.RefreshToken);
            return existingRefreshToken!;
        }

        #endregion
    }
}
