using Auth.BLL.Interface.Models;
using System;
using System.Security.Cryptography;

namespace Auth.Common
{
    public class RsaProvider
    {
        private readonly RSACryptoServiceProvider rsa;
        public RsaProvider(RSAParameters rsaParameters)
        {
            this.rsa = new RSACryptoServiceProvider();
            this.rsa.ImportParameters(rsaParameters);
        }

        public RsaProvider(RsaPublicKey publicKey)
        {
            var parameters = new RSAParameters();
            parameters.Modulus = publicKey.Modulus;
            parameters.Exponent = publicKey.Exponent;
            this.rsa = new RSACryptoServiceProvider();
            this.rsa.ImportParameters(parameters);
        }

        public RsaProvider(RsaKey key)
        {
            this.rsa = new RSACryptoServiceProvider();
            var parameters = new RSAParameters
            {
                Exponent = key.Exponent,
                D = key.D,
                Modulus = key.Modulus,
                DP = key.DP,
                DQ = key.DQ,
                InverseQ = key.InverseQ,
                P = key.P,
                Q = key.Q,
            };
            this.rsa.ImportParameters(parameters);
        }

        public byte[] Decrypt(byte[] data)
        {
            return this.rsa.Decrypt(data, false);
        }

        public byte[] Encrypt(byte[] data)
        {
            return this.rsa.Encrypt(data, false);
        }
    }
}
