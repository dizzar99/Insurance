using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmailApp;
using Insurance.WebApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Insurance.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<User> userManager;

        public UsersController(UserManager<User> userManager)
        {
            this.userManager = userManager;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Register(User user)
        {
            // добавляем пользователя
            var result = await userManager.CreateAsync(user, "!QAZxsw2");
            if (result.Succeeded)
            {
                // генерация токена для пользователя
                var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
                var callbackUrl = Url.Action(
                    "ConfirmEmail",
                    "Users",
                    new { userId = user.Id, code },
                    protocol: HttpContext.Request.Scheme);
                EmailService emailService = new EmailService();
                await emailService.SendMailAsync(user.Email, "Confirm your account",
                    $"Confirm your email address: {callbackUrl}");

                return Content("Для завершения регистрации проверьте электронную почту и перейдите по ссылке, указанной в письме");
            }

            return BadRequest();
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {

            var user = await userManager.FindByIdAsync(userId);

            var result = await userManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded)
                return Ok("GOOD");

            return BadRequest("BAD");
        }
    }
}
