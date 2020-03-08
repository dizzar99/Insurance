using Auth.BLL.Interface.Models.SessionModels;
using Auth.Common.Interface;
using System.Numerics;
using System.Text;

namespace Auth.Common.Extensions
{
    public static class DigitalSignatureExtensions
    {
        public static (byte[] privateKey, ECPoint publicKey) GenerateParameters(this IDigitalSignature signature)
        {
            var parameters = signature.GenerateKeyPair();
            var privateKey = parameters.privateKey.ToByteArray();
            var publicKey = new ECPoint
            {
                X = parameters.publicKey.X.ToByteArray(),
                Y = parameters.publicKey.Y.ToByteArray(),
            };

            return (privateKey, publicKey);
        }

        public static (byte[] r, byte[] s) Sign(this IDigitalSignature signature, byte[] byteMessage, byte[] bytePrivateKey)
        {
            string message = Encoding.UTF8.GetString(byteMessage);
            BigInteger privateKey = new BigInteger(bytePrivateKey);
            var sign = signature.Sign(message, privateKey);
            var r = sign.r.ToByteArray();
            var s = sign.s.ToByteArray();

            return (r, s);
        }

        public static bool Verify(this IDigitalSignature ds, byte[] byteMessage, (byte[] r, byte[] s) byteSignature, ECPoint bytePublicKey)
        {
            string message = Encoding.UTF8.GetString(byteMessage);
            BigInteger r = new BigInteger(byteSignature.r);
            BigInteger s = new BigInteger(byteSignature.s);
            var signature = (r, s);
            BigIntegerPoint publicKey = new BigIntegerPoint
            {
                X = new BigInteger(bytePublicKey.X),
                Y = new BigInteger(bytePublicKey.Y),
            };

            return ds.Verify(message, signature, publicKey);
        }
    }
}
