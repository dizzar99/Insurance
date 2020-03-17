using Insurance.BLL.Interface.Exceptions.SessionManagment;
using Insurance.BLL.Interface.Extensions;
using Insurance.BLL.Interface.Interfaces;
using Insurance.BLL.Interface.Models.SessionModels;
using Insurance.Common.Exceptions.Aes;
using Insurance.Common.Implementation;
using Insurance.Common.Interface;
using Insurance.DataAccess;
using Insurance.DataAccess.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Threading.Tasks;

namespace Insurance.BLL.Implementations
{
    public class SessionService : ISessionService
    {
        private const int AllowedUsageCount = 5;
        private const int AllowedHours = 1;
        private readonly ApplicationContext context;
        private readonly IServerKeyManager keyManager;
        private readonly IMapper mapper;
        private readonly EllipticCurve ellipticCurve;

        public SessionService(ApplicationContext context, IServerKeyManager keyManager, IMapper mapper, EllipticCurve ellipticCurve)
        {
            this.context = context;
            this.keyManager = keyManager;
            this.mapper = mapper;
            this.ellipticCurve = ellipticCurve;
        }

        private DateTime ExpiredDate
        {
            get => DateTime.UtcNow + new TimeSpan(0, 1, 0);
        }

        public async Task<byte[]> GetSessionKey(int sessionId)
        {
            var dbSession = await this.GetDbSession(sessionId);
            this.ValidateSession(dbSession);

            dbSession.AllowedUsageCount--;
            this.context.Sessions.Update(dbSession);
            await this.context.SaveChangesAsync();

            return dbSession.SessionKey;
        }

        public async Task<ServerHello> CreateSession(ClientHello clientHello)
        {
            var serverRandom = this.GenerateRandomSecret(32);
            var dbClientHello = this.mapper.Map<DbClientHello>(clientHello);

            var dh = new DiffieHellman(this.ellipticCurve);
            var keyPair = dh.GenerateByteKeyPair();

            var publicKey = this.mapper.Map<DbECPoint>(keyPair.publicKey);

            var dbServerHello = new DbServerHello
            {
                PrivateKey = keyPair.privateKey,
                PublicKey = publicKey,
                ServerRandom = serverRandom,
            };

            var dbSession = new DbSession
            {
                AllowedUsageCount = AllowedUsageCount,
                ClientHello = dbClientHello,
                Confirmed = false,
                ServerHello = dbServerHello,
                Created = DateTime.UtcNow
            };

            this.context.Sessions.Add(dbSession);
            await this.context.SaveChangesAsync();

            var serverHello = this.mapper.Map<ServerHello>(dbServerHello);
            await this.SignServerHello(serverHello);

            return serverHello;
        }

        public async Task GenerateMasterKey(int sessionId, ECPoint clientPublicKey)
        {
            var dbSession = await this.context.Sessions
                .Include(s => s.ClientHello)
                .Include(s => s.MasterKey)
                .Include(s => s.ServerHello)
                .FirstAsync(s => s.Id == sessionId);

            var dh = new DiffieHellman(this.ellipticCurve);

            var secret = dh.GetSharedKey(dbSession.ServerHello.PrivateKey, clientPublicKey);
            var masterKey = this.mapper.Map<DbECPoint>(secret);

            dbSession.MasterKey = masterKey;
            dbSession.SessionKey = this.GenerateKey(dbSession.ClientHello.ClientRandom, dbSession.ServerHello.ServerRandom, masterKey);
            dbSession.Confirmed = true;
            dbSession.Expired = this.ExpiredDate;

            this.context.Sessions.Update(dbSession);
            await this.context.SaveChangesAsync();
        }

        public async Task<byte[]> RefreshSession(int sessionId, byte[] secret)
        {
            var dbSession = await this.GetDbSession(sessionId);
            var aesProvider = new AesProvider(dbSession.SessionKey);
            try
            {
                var clientRandomDecrypted = aesProvider.Decrypt(secret);
            }
            catch (AesException)
            {
                throw new InvalidClientSecretException();
            }

            var newSecret = this.GenerateRandomSecret(32);

            dbSession.SessionKey = newSecret;
            dbSession.AllowedUsageCount = AllowedUsageCount;
            dbSession.Expired = this.ExpiredDate;

            this.context.Sessions.Update(dbSession);
            await this.context.SaveChangesAsync();

            var encryptedNewSecret = aesProvider.Encrypt(newSecret);
            return encryptedNewSecret;
        }

        private async Task SignServerHello(ServerHello serverHello)
        {
            var privateKey = await this.keyManager.GetPrivateKey();
            var toSign = Helper.ConcatenateArrays(serverHello.ServerRandom, serverHello.PublicKey.X, serverHello.PublicKey.Y);
            var ecdsa = new EllipticCurveDSA(this.ellipticCurve);
            var (r, s) = ecdsa.Sign(toSign, privateKey);
            serverHello.Signature = new ECDigitalSignature
            {
                R = r,
                S = s,
            };
        }

        private byte[] GenerateRandomSecret(int bufSize)
        {
            var buffer = new byte[bufSize];
            var random = new Random();
            random.NextBytes(buffer);
            return buffer;
        }

        private async Task<DbSession> GetDbSession(int sessionId)
        {
            var dbSession = await this.context.Sessions
                .Include(s => s.ClientHello)
                .Include(s => s.MasterKey)
                .Include(s => s.ServerHello)
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (dbSession is null)
            {
                throw new SessionNotFoundException();
            }

            return dbSession;
        }

        private void ValidateSession(DbSession dbSession)
        {
            if (!dbSession.Confirmed)
            {
                throw new SessionNotConfirmedException();
            }

            if (dbSession.Expired < DateTime.UtcNow)
            {
                throw new SessionKeyExpiredException();
            }

            if (dbSession.AllowedUsageCount == 0)
            {
                throw new SessionKeyExpiredException();
            }
        }

        private byte[] GenerateKey(byte[] clientRandom, byte[] serverRandom, DbECPoint masterKey)
        {
            return Helper.ExclusiveOr(clientRandom, serverRandom, masterKey.X);
        }
    }
}
