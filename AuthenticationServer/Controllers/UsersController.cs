using AuthenticationServer.Domain;
using Insurance.BLL.Interface.Interfaces;
using Insurance.BLL.Interface.Models.IdentityModels.Requests;
using Insurance.BLL.Interface.Models.MailManagment;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AuthenticationServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IIdentityService identityService;
        private readonly IMailSender sender;

        public UsersController(IIdentityService identityService, IMailSender sender)
        {
            this.identityService = identityService;
            this.sender = sender;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterUserRequest registerUser)
        {
            var parameters = await this.identityService.RegisterAsync(registerUser);
            var callbackUrl = this.Url.Action(
                    "ConfirmEmail",
                    "Users",
                    new { userId = parameters.Id, parameters.Code },
                    protocol: this.HttpContext.Request.Scheme);

            var message = new EmailMessage
            {
                To = parameters.Email,
                Subject = "Confirm your account",
                Content = $"Confirm your email address: {callbackUrl}"
            };

            await this.sender.SendMailAsync(message);
            return this.Ok(parameters);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginUserRequest loginUser)
        {
            var authResult = await this.identityService.LoginAsync(loginUser);

            return this.Ok(new AuthSuccessResponse
            {
                Token = authResult.Token,
            });
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            var authResult = await this.identityService.ConfirmEmail(userId, code);
            return this.Ok(new AuthSuccessResponse
            {
                Token = authResult.Token,
            });
        }
    }
}