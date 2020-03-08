using Auth.BLL.Interface.Interfaces;
using Auth.BLL.Interface.Models;
using Auth.DataAccess;
using Auth.DataAccess.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using ECPoint = Auth.BLL.Interface.Models.SessionModels.ECPoint;

namespace Auth.BLL.Implementations
{
    public class DefaultKeyManager : IServerKeyManager
    {
        private readonly ApplicationContext context;
        private readonly IMapper mapper;

        public DefaultKeyManager(ApplicationContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        public async Task<byte[]> GetPrivateKey()
        {
            var dbECDSAKey = await this.context.ServerKeys
                .OrderByDescending(db => db.Id)
                .FirstAsync();

            return dbECDSAKey.PrivateKey;
        }

        public async Task<ECPoint> GetPublicKey()
        {
            var dbECDSAKey = await this.context.ServerKeys
                .OrderByDescending(db => db.Id)
                .Include(db => db.PublicKey)
                .FirstAsync();
            return this.mapper.Map<ECPoint>(dbECDSAKey.PublicKey);
        }

        public async Task SetNewKey(byte[] privateKey, ECPoint publicKey)
        {
            var dbEcPoint = this.mapper.Map<DbECPoint>(publicKey);
            var dbECDSAKey = new DbECDSAKey
            {
                PrivateKey = privateKey,
                PublicKey = dbEcPoint,
                Expired = DateTime.UtcNow + new TimeSpan(1, 0, 0, 0),
            };

            this.context.ServerKeys.Add(dbECDSAKey);
            await this.context.SaveChangesAsync();
        }

        private byte[] Encrypt(byte[] toEncrypt, RsaPublicKey publicKey)
        {
            using var rsa = new RSACryptoServiceProvider();
            var parameters = new RSAParameters();
            parameters.Modulus = publicKey.Modulus;
            parameters.Exponent = publicKey.Exponent;
            rsa.ImportParameters(parameters);
            var result = rsa.Encrypt(toEncrypt, false);
            return result;
        }
    }
}
