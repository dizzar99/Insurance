using AutoMapper;
using Insurance.BLL.Interface.Exceptions.IdentityManagment;
using Insurance.BLL.Interface.Exceptions.IdentityManagment.RefreshTokens;
using Insurance.BLL.Interface.Interfaces;
using Insurance.BLL.Interface.Models.IdentityModels.Requests;
using Insurance.BLL.Interface.Models.IdentityModels.Responses;
using Insurance.BLL.Interface.Options;
using Insurance.DataAccess;
using Insurance.DataAccess.IdentityModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Insurance.BLL.Implementations
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly IMapper mapper;
        private readonly JwtSettings jwtSettings;
        private readonly TokenValidationParameters validationParameters;
        private readonly IdentityContext identityContext;

        public IdentityService(
            UserManager<IdentityUser> userManager,
            IMapper mapper,
            JwtSettings jwtSettings,
            TokenValidationParameters validationParameters,
            IdentityContext identityContext)
        {
            this.userManager = userManager;
            this.mapper = mapper;
            this.jwtSettings = jwtSettings;
            this.validationParameters = validationParameters;
            this.identityContext = identityContext;
        }

        public async Task<AuthenticationResult> LoginAsync(LoginUserRequest loginUser)
        {
            var user = await this.userManager.FindByNameAsync(loginUser.UserName);
            await this.ValidateUserLogin(loginUser, user);

            return await this.GenerateAuthenticationResultForUserAsync(user);
        }

        public async Task<ConfirmationEmailParameters> RegisterAsync(RegisterUserRequest registerUser)
        {
            var existingUser = await this.userManager.FindByNameAsync(registerUser.UserName);
            if (existingUser != null)
            {
                throw new UserNameConflictException();
            }

            var newUser = this.mapper.Map<IdentityUser>(registerUser);
            var creationUserResult = await this.userManager.CreateAsync(newUser, registerUser.Password);
            if (!creationUserResult.Succeeded)
            {
                throw new UserNameConflictException();
            }

            var code = await this.userManager.GenerateEmailConfirmationTokenAsync(newUser);
            string userId = newUser.Id;

            return new ConfirmationEmailParameters
            {
                Id = userId,
                Code = code,
                Email = newUser.Email,
            };
        }

        public async Task<AuthenticationResult> RefreshTokenAsync(string token, string refreshToken)
        {
            var validatedToken = this.GetPrincipalFromExpiredToken(token);
            this.ValidateAccessToken(validatedToken);

            var dbRefreshToken = await this.identityContext.RefreshTokens.FirstOrDefaultAsync(x => x.Token == refreshToken);
            this.ValidateRefreshToken(dbRefreshToken, validatedToken);

            dbRefreshToken.Used = true;
            this.identityContext.RefreshTokens.Update(dbRefreshToken);
            await this.identityContext.SaveChangesAsync();

            var user = await this.userManager.FindByIdAsync(validatedToken.Claims.Single(c => c.Type == "id").Value);

            return await this.GenerateAuthenticationResultForUserAsync(user);
        }

        public async Task<AuthenticationResult> ConfirmEmail(string userId, string code)
        {
            var user = await this.userManager.FindByIdAsync(userId);

            var result = await this.userManager.ConfirmEmailAsync(user, code);
            if (!result.Succeeded)
            {
                throw new EmailConfirmationException(result.Errors.First().Description);
            }

            return await this.GenerateAuthenticationResultForUserAsync(user);
        }

        private async Task<AuthenticationResult> GenerateAuthenticationResultForUserAsync(IdentityUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = this.GenerateJwtToken(user);
            var refreshToken = new DbRefreshToken
            {
                Token = this.GenerateRefreshToken(),
                JwtId = token.Id,
                UserId = user.Id,
                CreationDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(10),
            };

            await this.identityContext.RefreshTokens.AddAsync(refreshToken);
            await this.identityContext.SaveChangesAsync();

            return new AuthenticationResult
            {
                Success = true,
                Token = tokenHandler.WriteToken(token),
                RefreshToken = refreshToken.Token,
            };
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                this.validationParameters.ValidateLifetime = false;
                var principal = tokenHandler.ValidateToken(token, this.validationParameters, out var _);
                this.validationParameters.ValidateLifetime = true;
                return principal;
            }
            catch
            {
                throw new InvalidTokenException();
            }
        }

        private SecurityToken GenerateJwtToken(IdentityUser user)
        {
            var tokenHandker = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(this.jwtSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("id", user.Id),
                }),
                Expires = DateTime.UtcNow.Add(this.jwtSettings.TokenLifeTime),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256),
            };

            var token = tokenHandker.CreateToken(tokenDescriptor);

            return token;
        }

        private async Task ValidateUserLogin(LoginUserRequest loginUser, IdentityUser user)
        {
            if (user is null)
            {
                throw new UserNotFoundException();
            }

            var userConfirmedEmail = await this.userManager.IsEmailConfirmedAsync(user);
            if (!userConfirmedEmail)
            {
                throw new EmailConfirmationException("User does not confirm email.");
            }

            bool userHasValidPasword = await this.userManager.CheckPasswordAsync(user, loginUser.Password);
            if (!userHasValidPasword)
            {
                throw new InvalidPasswordException();
            }
        }

        private void ValidateRefreshToken(DbRefreshToken refreshToken, ClaimsPrincipal claimsIdentity)
        {
            var jti = claimsIdentity.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Jti).Value;
            var userId = claimsIdentity.Claims.Single(c => c.Type == "id").Value;

            if (refreshToken is null)
            {
                throw new RefreshTokenNotFoundException();
            }

            if (DateTime.UtcNow > refreshToken.ExpiryDate)
            {
                throw new RefreshTokenExpiredException();
            }

            if (refreshToken.Used)
            {
                throw new InvalidTokenException("This token has already been used.");
            }

            if (refreshToken.JwtId != jti)
            {
                throw new RefreshTokenConflictException();
            }
        }

        private void ValidateAccessToken(ClaimsPrincipal validatedToken)
        {
            var expiryDateUnix = long.Parse(validatedToken.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Exp).Value);
            var expiryDateTimeUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                .AddSeconds(expiryDateUnix);

            if (expiryDateTimeUtc > DateTime.UtcNow)
            {
                throw new InvalidTokenException("Access token has not been expired.");
            }
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
