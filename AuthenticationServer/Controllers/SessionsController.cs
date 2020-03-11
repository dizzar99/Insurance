using Insurance.BLL.Interface.Interfaces;
using Insurance.BLL.Interface.Models.SessionModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace AuthenticationServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SessionsController : ControllerBase
    {
        private readonly ISessionService sessionService;

        public SessionsController(ISessionService sessionService)
        {
            this.sessionService = sessionService;
        }

        [HttpPost("hello")]
        public async Task<ServerHello> GetServerHello(ClientHello clientHello)
        {
            var result = await this.sessionService.CreateSession(clientHello);
            return result;
        }

        [HttpPost("{id}/master")]
        public async Task GenerateSessionKey(int id, ECPoint clientPublicKey)
        {
            await this.sessionService.GenerateMasterKey(id, clientPublicKey);
        }

        [HttpPost("{id}/refresh")]
        public async Task<byte[]> RefreshSessionKey(int id, byte[] secret)
        {
            var result = await this.sessionService.RefreshSession(id, secret);

             return result;
        }
    }
}