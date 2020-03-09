using Auth.BLL.Interface.Interfaces;
using Auth.BLL.Interface.Models.SessionModels;
using Auth.Common.Extensions;
using Auth.Common.Implementation;
using Auth.Common.Interface;
using Auth.DataAccess;
using Auth.DataAccess.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;

namespace Auth.BLL.Implementations
{
    public class SessionService : ISessionService
    {
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

        public async Task<bool> ConfirmSession(int sessionId, byte[] secret)
        {
            var dbSession = await this.GetSession(sessionId);
            if (dbSession.Confirmed)
            {
                return true;
            }

            if (this.CompareBytes(dbSession.SessionKey, secret))
            {
                dbSession.Confirmed = true;
                this.context.Sessions.Update(dbSession);
                await this.context.SaveChangesAsync();
                return true;
            }

            await this.DeclaimSession(sessionId);
            return false;
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
                AllowedUsageCount = 5,
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

        public async Task<byte[]> GenerateMasterKey(int sessionId, ECPoint clientPublicKey)
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

            this.context.Sessions.Update(dbSession);
            await this.context.SaveChangesAsync();

            return dbSession.SessionKey;
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

        public async Task DeclaimSession(int sessionId)
        {
            var dbSession = await this.GetSession(sessionId);

            this.context.Sessions.Remove(dbSession);
        }

        public async Task<byte[]> RefreshSession(int sessionId, byte[] secret)
        {
            var dbSession = await this.GetSession(sessionId);
            var aesProvider = new AesProvider(dbSession.SessionKey);
            var clientRandomDecrypted = aesProvider.Decrypt(secret);
            if (!dbSession.ClientHello.ClientRandom.SequenceEqual(clientRandomDecrypted))
            {
                // TODO: Invalid clientRandomSecret
                throw new Exception();
            }

            var newSecret = this.GenerateRandomSecret(32);
            dbSession.SessionKey = newSecret;
            this.context.Sessions.Update(dbSession);
            await this.context.SaveChangesAsync();

            var encryptedNewSecret = aesProvider.Encrypt(newSecret);
            return encryptedNewSecret;
        }

        private byte[] GenerateRandomSecret(int bufSize)
        {
            var buffer = new byte[bufSize];
            var random = new Random();
            random.NextBytes(buffer);
            return buffer;
        }

        private async Task<DbSession> GetSession(int sessionId)
        {
            var dbSession = await this.context.Sessions
                .Include(s => s.ClientHello)
                .Include(s => s.MasterKey)
                .Include(s => s.ServerHello)
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (dbSession is null)
            {
                // TODO: sessionNotFoundException.
            }

            return dbSession;
        }

        private bool CompareBytes(byte[] lhs, byte[] rhs)
        {
            return StructuralComparisons.StructuralEqualityComparer.Equals(lhs, rhs);
        }

        private byte[] GenerateKey(byte[] clientRandom, byte[] serverRandom, DbECPoint masterKey)
        {
            return Helper.ExclusiveOr(clientRandom, serverRandom, masterKey.X);
        }
    }
}
