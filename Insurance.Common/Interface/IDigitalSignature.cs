using System.Numerics;

namespace Insurance.Common.Interface
{
    public interface IDigitalSignature
    {
        (BigInteger privateKey, BigIntegerPoint publicKey) GenerateKeyPair();
        (BigInteger r, BigInteger s) Sign(string message, BigInteger privateKey);
        bool Verify(string message, (BigInteger r, BigInteger s) signature, BigIntegerPoint publicKey);
    }
}
