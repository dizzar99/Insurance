using Insurance.BLL.Interface.Models.IdentityModels.Requests;
using Insurance.BLL.Interface.Models.IdentityModels.Responses;
using System.Threading.Tasks;

namespace Insurance.BLL.Interface.Interfaces
{
    public interface IIdentityService
    {
        Task<ConfirmationParameters> RegisterAsync(RegisterUserRequest createUser);
        Task<AuthenticationResult> LoginAsync(LoginUserRequest loginUser);
        Task<AuthenticationResult> ConfirmEmail(string userId, string code);
    }
}
