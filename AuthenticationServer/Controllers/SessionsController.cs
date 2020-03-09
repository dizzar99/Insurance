using Auth.BLL.Interface.Interfaces;
using Auth.BLL.Interface.Models;
using Auth.BLL.Interface.Models.SessionModels;
using Auth.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SessionsController : ControllerBase
    {
        private readonly ISessionService sessionService;
        private readonly ILogger logger;

        public SessionsController(ISessionService sessionService, ILogger<SessionsController> logger)
        {
            this.sessionService = sessionService;
            this.logger = logger;
        }

        //[HttpGet]
        //public async Task<DbRsaKey> AddServerKey()
        //{
        //    var rsa = new RSACryptoServiceProvider(2048);
        //    var parameters = rsa.ExportParameters(true);
        //    var db = new DbRsaKey
        //    {
        //        Exponent = parameters.Exponent,
        //        Modulus = parameters.Modulus,
        //        D = parameters.D,
        //        P = parameters.P,
        //        Q = parameters.Q,
        //        DP = parameters.DP,
        //        DQ = parameters.DQ,
        //        InverseQ = parameters.DQ,
        //        ExpiredDate = DateTime.Now + new TimeSpan(100, 0, 0, 0),
        //    };

        //    var result = this.context.ServerKeys.Add(db);
        //    await this.context.SaveChangesAsync();
        //    return result.Entity;
        //}

        [HttpPost("hello")]
        public async Task<ServerHello> GetServerHello(ClientHello clientHello)
        {
            //var clientRandom = new byte[32];
            //var random = new Random();
            //random.NextBytes(clientRandom);
            //var clientHello = new ClientHello
            //{
            //    ClientRandom = clientRandom,
            //};

            var result = await this.sessionService.CreateSession(clientHello);
            return result;
        }

        [HttpPost("{id}/master")]
        public async Task GetMasterKey(int id, ECPoint clientPublicKey)
        {
            var masterKey = await this.sessionService.GenerateMasterKey(id, clientPublicKey);
            this.logger.LogError(Convert.ToBase64String(masterKey));
        }

        [HttpPost("{id}/refresh")]
        public async Task<byte[]> RefreshSessionKey(int id, byte[] secret)
        {
            var result = await this.sessionService.RefreshSession(id, secret);

            return result;
        }

        //[HttpGet]
        //public async Task<RsaPublicKey> GetServerKey()
        //{
        //    return await this.keyManager.GetPublicKey();
        //}

        //[HttpPost]
        //public async Task<SymmetricKey> CreateSession(byte[] encryptedPublicKey)
        //{
        //    var bytes = encryptedPublicKey;
        //    var bb = Convert.ToBase64String(bytes);
        //    var parameters = await this.GetParameters(bytes);
        //    var session = await this.sessionService.CreateSession();
        //    var sessionKey = await this.sessionService.GetSessionKey(session.Id, true);

        //    var rrr = Convert.ToBase64String(sessionKey.Key);
        //    this.EncryptSymmetricKey(sessionKey, parameters);
        //    return sessionKey;
        //}

        private void EncryptSymmetricKey(SymmetricKey key, RsaPublicKey publicKey)
        {
            var rsa = new RsaProvider(publicKey);
            key.Key = rsa.Encrypt(key.Key);
        }
    }
}