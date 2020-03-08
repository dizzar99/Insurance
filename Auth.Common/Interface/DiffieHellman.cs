using System;
using System.Numerics;

namespace Auth.Common.Interface
{
    public class DiffieHellman
    {
        public EllipticCurve Curve { get; }
        public DiffieHellman(EllipticCurve curve)
        {
            this.Curve = curve ?? throw new ArgumentNullException(nameof(curve));
        }

        public (BigInteger privateKey, BigIntegerPoint publicKey) GenerateKeyPair()
        {
            BigInteger privateKey = EllipticCurveHelpers.RandomIntegerBelow(this.Curve.N);
            BigIntegerPoint publicKey = this.Curve.ScalarMult(privateKey, this.Curve.G);
            return (privateKey, publicKey);
        }
        public BigIntegerPoint GetSharedkey(BigInteger privateKey, BigIntegerPoint publicKey)
        {
            return this.Curve.ScalarMult(privateKey, publicKey);
        }
    }
}
