using Insurance.BLL.Interface.Models.SessionModels;
using Insurance.Common.Interface;
using System.Numerics;

namespace Insurance.BLL.Interface.Extensions
{
    public static class DiffieHellmanExtensions
    {
        public static (byte[] privateKey, ECPoint publicKey) GenerateByteKeyPair(this DiffieHellman dh)
        {
            var keyPair = dh.GenerateKeyPair();
            var privateKey = keyPair.privateKey.ToByteArray();
            var publicKey = new ECPoint
            {
                X = keyPair.publicKey.X.ToByteArray(),
                Y = keyPair.publicKey.Y.ToByteArray(),
            };

            return (privateKey, publicKey);
        }

        public static ECPoint GetSharedKey(this DiffieHellman dh, byte[] bytePrivateKey, ECPoint bytePublicKey)
        {
            var privateKey = new BigInteger(bytePrivateKey);
            var publicKey = new BigIntegerPoint
            {
                X = new BigInteger(bytePublicKey.X),
                Y = new BigInteger(bytePublicKey.Y),
            };

            var sharedKey = dh.GetSharedkey(privateKey, publicKey);
            var result = new ECPoint
            {
                X = sharedKey.X.ToByteArray(),
                Y = sharedKey.Y.ToByteArray(),
            };

            return result;
        }
    }
}
