using System;
using System.Collections.Generic;
using System.Numerics;

namespace Auth.Common.Interface
{
    public class EllipticCurve
    {
        public EllipticCurve(BigInteger a, BigInteger b, BigInteger p, BigIntegerPoint g, BigInteger n)
        {
            this.A = a;
            this.B = b;
            this.P = p;
            this.G = g;
            this.N = n;
        }

        public EllipticCurve() { }
        //  Recommended parameters secp256r1. Reference: http://www.secg.org/SEC2-Ver-1.0.pdf

        // Field characteristic.
        public BigInteger P { get; } = BigInteger.Parse("0FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFEFFFFFC2F", System.Globalization.NumberStyles.HexNumber);

        // Curve coefficients.
        public BigInteger A { get; } = BigInteger.Parse("0", System.Globalization.NumberStyles.HexNumber);

        public BigInteger B { get; } = BigInteger.Parse("07", System.Globalization.NumberStyles.HexNumber);

        // Base point.
        public BigIntegerPoint G { get; } = new BigIntegerPoint(
            BigInteger.Parse("079BE667EF9DCBBAC55A06295CE870B07029BFCDB2DCE28D959F2815B16F81798", System.Globalization.NumberStyles.HexNumber),
            BigInteger.Parse("0483ADA7726A3C4655DA4FBFC0E1108A8FD17B448A68554199C47D08FFB10D4B8", System.Globalization.NumberStyles.HexNumber));

        // Subgroup order.
        public BigInteger N { get; } = BigInteger.Parse("0FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFEBAAEDCE6AF48A03BBFD25E8CD0364141", System.Globalization.NumberStyles.HexNumber);


        public IEnumerable<BigIntegerPoint> GenerateAllPoints()
        {
            for (BigInteger x = 0; x < this.P; x++)
            {
                BigInteger ySquare = BigInteger.ModPow(x, 3, this.P) + this.A * x + this.B;
                bool isSquare = EllipticCurveHelpers.Legendre(ySquare, this.P) != -1;
                if (isSquare)
                {
                    var y = EllipticCurveHelpers.ShanksSqrt(ySquare, this.P);
                    yield return new BigIntegerPoint(x, y);
                    yield return new BigIntegerPoint(x, EllipticCurveHelpers.MathMod(-y, this.P));
                }
            }
        }

        /// <summary>
        /// Returns k * point computed using the double and <see cref="PointAdd"/> algorithm.
        /// </summary>
        /// <param name="k"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public BigIntegerPoint ScalarMult(BigInteger k, BigIntegerPoint point)
        {
            if (EllipticCurveHelpers.MathMod(k, this.N) == 0 || point.IsEmpty())
            {
                return BigIntegerPoint.GetEmpty();
            }

            if (k < 0)
            {
                return this.ScalarMult(-k, this.NegatePoint(point));
            }

            BigIntegerPoint result = BigIntegerPoint.GetEmpty();
            BigIntegerPoint addend = point;

            while (k > 0)
            {
                if ((k & 1) > 0)
                {
                    result = this.PointAdd(result, addend);
                }

                addend = this.PointAdd(addend, addend);

                k >>= 1;
            }

            return result;
        }

        /// <summary>
        /// Returns the result of lhs + rhs according to the group law.
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public BigIntegerPoint PointAdd(BigIntegerPoint lhs, BigIntegerPoint rhs)
        {
            if (lhs.IsEmpty())
            {
                return rhs;
            }

            if (rhs.IsEmpty())
            {
                return lhs;
            }

            BigInteger x1 = lhs.X, y1 = lhs.Y;
            BigInteger x2 = rhs.X, y2 = rhs.Y;

            if (x1 == x2 && y1 != y2)
            {
                return BigIntegerPoint.GetEmpty();
            }

            BigInteger m;
            if (x1 == x2)
            {
                m = (3 * x1 * x1 + this.A) * this.InverseMod(2 * y1, this.P);
            }
            else
            {
                m = (y1 - y2) * this.InverseMod(x1 - x2, this.P);
            }

            BigInteger x3 = m * m - x1 - x2;
            BigInteger y3 = y1 + m * (x3 - x1);
            BigIntegerPoint result = new BigIntegerPoint(EllipticCurveHelpers.MathMod(x3, this.P), EllipticCurveHelpers.MathMod(-y3, this.P));

            return result;
        }

        /// <summary>
        /// Returns the inverse of k modulo p.
        /// </summary>
        /// <param name="k">Must be non-zero</param>
        /// <param name="p">Must be a prime</param>
        /// <returns></returns>
        public BigInteger InverseMod(BigInteger k, BigInteger p)
        {
            if (k == 0)
            {
                return BigInteger.Zero;
                //throw new DivideByZeroException(nameof(k));
            }

            if (k < 0)
            {
                return p - this.InverseMod(-k, p);
            }

            (BigInteger gcd, BigInteger x, BigInteger y) result = EllipticCurveHelpers.ExtendedGcd(p, k);

            if (result.gcd != 1)
            {
                throw new ArgumentException("Gcd is not 1");
            }

            if (EllipticCurveHelpers.MathMod(k * result.x, p) != 1)
            {
                throw new ArgumentException("(k * result.x) % p != 1");
            }

            return EllipticCurveHelpers.MathMod(result.x, p);
        }

        /// <summary>
        /// Returns -point.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public BigIntegerPoint NegatePoint(BigIntegerPoint point)
        {
            if (point.IsEmpty())
            {
                return point;
            }

            BigInteger x = point.X;
            BigInteger y = point.Y;
            BigIntegerPoint result = new BigIntegerPoint(x, EllipticCurveHelpers.MathMod(-y, this.P));

            return result;
        }

        /// <summary>
        /// Returns <c>true</c> if the given point lies on the elliptic curve.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool IsOnCurve(BigIntegerPoint point)
        {
            if (point.IsEmpty())
            {
                return true;
            }

            BigInteger x = point.X;
            BigInteger y = point.Y;

            return EllipticCurveHelpers.MathMod(y * y - x * x * x - this.A * x - this.B, this.P) == 0;
        }

        public override string ToString()
        {
            return $"y^2 = x^3 + {this.A}x + {this.B}";
        }
    }
}