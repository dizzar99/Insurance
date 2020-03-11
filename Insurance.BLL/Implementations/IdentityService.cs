using AutoMapper;
using Insurance.BLL.Interface.Exceptions.IdentityManagment;
using Insurance.BLL.Interface.Interfaces;
using Insurance.BLL.Interface.Models.IdentityModels.Requests;
using Insurance.BLL.Interface.Models.IdentityModels.Responses;
using Insurance.BLL.Interface.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Insurance.BLL.Implementations
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly IMapper mapper;
        private readonly JwtSettings jwtSettings;

        public IdentityService(UserManager<IdentityUser> userManager, IMapper mapper, JwtSettings jwtSettings)
        {
            this.userManager = userManager;
            this.mapper = mapper;
            this.jwtSettings = jwtSettings;
        }

        public async Task<AuthenticationResult> LoginAsync(LoginUserRequest loginUser)
        {
            var user = await this.userManager.FindByNameAsync(loginUser.UserName);
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

            var token = this.GenerateJwtToken(user);
            return new AuthenticationResult
            {
                Success = true,
                Token = token
            };
        }

        public async Task<ConfirmationParameters> RegisterAsync(RegisterUserRequest registerUser)
        {
            var existingUser = await this.userManager.FindByNameAsync(registerUser.UserName);
            if (existingUser != null)
            {
                throw new UserNameConflictException();
            }

            var newUser = this.mapper.Map<IdentityUser>(registerUser);
            var createdUser = await this.userManager.CreateAsync(newUser, registerUser.Password);
            if (!createdUser.Succeeded)
            {
                throw new UserNameConflictException();
            }

            var code = await this.userManager.GenerateEmailConfirmationTokenAsync(newUser);
            string userId = newUser.Id;

            return new ConfirmationParameters
            {
                Id = userId,
                Code = code,
                Email = newUser.Email,
            };
        }

        public async Task<AuthenticationResult> ConfirmEmail(string userId, string code)
        {
            var user = await this.userManager.FindByIdAsync(userId);

            var result = await this.userManager.ConfirmEmailAsync(user, code);
            if (!result.Succeeded)
            {
                throw new EmailConfirmationException(result.Errors.First().Description);
            }

            var token = this.GenerateJwtToken(user);
            return new AuthenticationResult
            {
                Success = true,
                Token = token,
            };
        }

        private string GenerateJwtToken(IdentityUser newUser)
        {
            var tokenHandker = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(this.jwtSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, newUser.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("id", newUser.Id),
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            };

            var token = tokenHandker.CreateToken(tokenDescriptor);

            return tokenHandker.WriteToken(token);
        }
    }
}
