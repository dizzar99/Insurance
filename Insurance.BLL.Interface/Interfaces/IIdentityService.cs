using Insurance.BLL.Interface.Models.IdentityModels.Requests;
using Insurance.BLL.Interface.Models.IdentityModels.Responses;
using System.Threading.Tasks;

namespace Insurance.BLL.Interface.Interfaces
{
    public interface IIdentityService
    {
        Task<ConfirmationEmailParameters> RegisterAsync(RegisterUserRequest createUser);
        Task<AuthenticationResult> LoginAsync(LoginUserRequest loginUser);
        Task<AuthenticationResult> ConfirmEmail(string userId, string code);
        Task<AuthenticationResult> RefreshTokenAsync(string token, string refreshToken);
    }
}
