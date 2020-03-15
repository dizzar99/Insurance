using Insurance.BLL.Interface.Extensions;
using Insurance.BLL.Interface.Interfaces;
using Insurance.Common.Implementation;
using Insurance.Common.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AuthenticationServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KeysController : ControllerBase
    {
        private readonly IServerKeyManager keyManager;

        public KeysController(IServerKeyManager keyManager)
        {
            this.keyManager = keyManager;
        }

        [HttpGet("new")]
        public async Task<IActionResult> GenerateNewKey()
        {
            var ecdsa = new EllipticCurveDSA(new EllipticCurve());
            var (privateKey, publicKey) = ecdsa.GenerateParameters();
            await this.keyManager.SetNewKey(privateKey, publicKey);

            return this.Ok();
        }

        [HttpGet("key")]
        public async Task<IActionResult> GetServerKey()
        {
            var result = await this.keyManager.GetPublicKey();

            return this.Ok(result);
        }
    }
}