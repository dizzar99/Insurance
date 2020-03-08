using Auth.Common.Interface;
using System;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace Auth.Common.Implementation
{
    public class EllipticCurveDSA : IDigitalSignature
    {
        public EllipticCurve Curve { get; set; }

        public EllipticCurveDSA(EllipticCurve curve)
        {
            this.Curve = curve;
        }

        /// <summary>
        /// Generates a random private-public key pair.
        /// </summary>
        /// <returns>The tuple consists of a private and public key</returns>
        public (BigInteger privateKey, BigIntegerPoint publicKey) GenerateKeyPair()
        {
            BigInteger privateKey = EllipticCurveHelpers.RandomIntegerBelow(this.Curve.N);
            BigIntegerPoint publicKey = this.Curve.ScalarMult(privateKey, this.Curve.G);
            return (privateKey, publicKey);
        }

        /// <summary>
        /// Generates signature.
        /// </summary>
        /// <param name="privateKey"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public (BigInteger r, BigInteger s) Sign(string message, BigInteger privateKey)
        {
            BigInteger z = this.HashMessage(message);

            BigInteger r = 0;
            BigInteger s = 0;

            while (r == 0 || s == 0)
            {
                BigInteger k = EllipticCurveHelpers.RandomIntegerBelow(this.Curve.N);
                BigIntegerPoint point = this.Curve.ScalarMult(k, this.Curve.G);

                r = EllipticCurveHelpers.MathMod(point.X, this.Curve.N);
                s = EllipticCurveHelpers.MathMod((z + r * privateKey) * this.Curve.InverseMod(k, this.Curve.N), this.Curve.N);
            }

            return (r, s);
        }

        public bool Verify(string message, (BigInteger r, BigInteger s) signature, BigIntegerPoint publicKey)
        {
            BigInteger z = this.HashMessage(message);

            BigInteger w = this.Curve.InverseMod(signature.s, this.Curve.N);
            BigInteger u1 = EllipticCurveHelpers.MathMod((z * w), this.Curve.N);
            BigInteger u2 = EllipticCurveHelpers.MathMod((signature.r * w), this.Curve.N);

            BigIntegerPoint point = this.Curve.PointAdd(this.Curve.ScalarMult(u1, this.Curve.G), this.Curve.ScalarMult(u2, publicKey));
            return EllipticCurveHelpers.MathMod(signature.r, this.Curve.N) == EllipticCurveHelpers.MathMod(point.X, this.Curve.N);
        }

        /// <summary>
        /// Generates the truncated SHA521 hash of the <paramref name="message"/>.
        /// </summary>
        /// <param name="message"></param>
        private BigInteger HashMessage(string message)
        {
            byte[] messageBytes = Encoding.Default.GetBytes(message);
            byte[] messageHash;

            using (SHA512Managed sha512M = new SHA512Managed())
            {
                messageHash = sha512M.ComputeHash(messageBytes);
            }

            BigInteger e = new BigInteger(messageHash);
            BigInteger z = e >> (e.BitLenght() - this.Curve.N.BitLenght());

            if (!(z.BitLenght() <= this.Curve.N.BitLenght()))
            {
                throw new Exception("!(z.BitLenght() <= Curve.n.BitLenght())");
            }

            return z;
        }
    }
}