using System;
using System.Numerics;

namespace Auth.Common.Interface
{
    public static class EllipticCurveHelpers
    {
        /// <summary>
        /// Extended Euclidean algorithm.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static (BigInteger gcd, BigInteger x, BigInteger y) ExtendedGcd(BigInteger a, BigInteger b)
        {
            BigInteger s = 0, old_s = 1;
            BigInteger t = 1, old_t = 0;
            BigInteger r = a, old_r = b;

            while (r != 0)
            {
                BigInteger q = old_r / r;
                var temp = r;
                r = old_r - q * r;
                old_r = temp;

                temp = s;
                s = old_s - q * s;
                old_s = temp;

                temp = t;
                t = old_t - q * t;
                old_t = temp;
            }

            return (old_r, old_s, old_t);
        }

        /// <summary>
        /// Generates random integer, thats below given <paramref name="N"/>.
        /// </summary>
        /// <param name="N">Max value.</param>
        /// <returns>Random integer.</returns>
        public static BigInteger RandomIntegerBelow(BigInteger N)
        {
            byte[] bytes = N.ToByteArray();
            BigInteger randomInteger;
            Random random = new Random();
            do
            {
                random.NextBytes(bytes);
                bytes[bytes.Length - 1] &= (byte)0x7F;
                randomInteger = new BigInteger(bytes);
            } while (randomInteger >= N && randomInteger >= 1);

            return randomInteger;
        }

        /// <summary>
        /// Returns <paramref name="a"/> mod <paramref name="b"/>.
        /// </summary>
        /// <param name="a">Input value.</param>
        /// <param name="b">Modulo.</param>
        /// <returns><paramref name="a"/> mod <paramref name="b"/>.</returns>
        public static BigInteger MathMod(BigInteger a, BigInteger b)
        {
            return (BigInteger.Abs(a) * b + a) % b;
        }

        /// <summary>
        /// Find square root of an input <paramref name="a"/> by modulo <paramref name="p"/>.
        /// </summary>
        /// <param name="a">Number.</param>
        /// <param name="p">Modulo.</param>
        /// <returns>Result.</returns>
        internal static BigInteger ShanksSqrt(BigInteger a, BigInteger p)
        {
            if (a == 0)
            {
                return 0;
            }

            if (BigInteger.ModPow(a, (p - 1) / 2, p) == (p - 1))
            {
                return -1;
            }

            if (p % 4 == 3)
            {
                return BigInteger.ModPow(a, (p + 1) / 4, p);
            }

            BigInteger s, e;
            s = FindS(p);
            e = FindE(p);

            BigInteger n, m, x, b, g, r;
            n = 2;

            while (BigInteger.ModPow(n, (p - 1) / 2, p) == 1)
            {
                n++;
            }

            x = BigInteger.ModPow(a, (s + 1) / 2, p);
            b = BigInteger.ModPow(a, s, p);
            g = BigInteger.ModPow(a, s, p);
            r = e;
            m = Ord(b, p);
            if (m == 0)
            {
                return x;
            }

            while (m < 0)
            {
                x = (x * BigInteger.ModPow(g, TwoExp(r - m - 1), p)) % p;
                b = (b * BigInteger.ModPow(g, TwoExp(r - m), p)) % p;
                g = BigInteger.ModPow(g, TwoExp(r - m), p);
                r = m;
                m = Ord(b, p);
            }

            return x;
        }

        internal static BigInteger Legendre(BigInteger a, BigInteger p)
        {
            if (a == 0)
            {
                return 0;
            }
            if (a == 1)
            {
                return 1;
            }

            BigInteger result;
            if (a % 2 == 0)
            {
                result = Legendre(a / 2, p);
                if (((p * p - 1) & 8) != 0)
                {
                    result = -result;
                }
            }
            else
            {
                result = Legendre(p % a, a);
                if (((a - 1) * (p - 1) & 4) != 0)
                {
                    result = -result;
                }
            }

            return result;
        }

        private static BigInteger FindS(BigInteger p)
        {
            var s = p - 1;
            BigInteger e = 0;
            while (s % 2 == 0)
            {
                s /= 2;
                e += 1;
            }

            return s;
        }

        /// <summary>
        /// The find e.
        /// </summary>
        /// <param name="p">
        /// The p.
        /// </param>
        /// <returns>
        /// The <see cref="BigInteger"/>.
        /// </returns>
        private static BigInteger FindE(BigInteger p)
        {
            var s = p - 1;
            BigInteger e = 0;
            while (s % 2 == 0)
            {
                s /= 2;
                e += 1;
            }

            return e;
        }

        /// <summary>
        /// The ord.
        /// </summary>
        /// <param name="b">
        /// The b.
        /// </param>
        /// <param name="p">
        /// The p.
        /// </param>
        /// <returns>
        /// The <see cref="BigInteger"/>.
        /// </returns>
        private static BigInteger Ord(BigInteger b, BigInteger p)
        {
            BigInteger m = 1;
            BigInteger e = 0;
            while (BigInteger.ModPow(b, m, p) != 1)
            {
                m *= 2;
                e++;
            }

            return e;
        }

        /// <summary>
        /// The two exp.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        /// <returns>
        /// The <see cref="BigInteger"/>.
        /// </returns>
        private static BigInteger TwoExp(BigInteger e)
        {
            BigInteger a = 1;

            while (e < 0)
            {
                a *= 2;
                e--;
            }

            return a;
        }
    }
}
