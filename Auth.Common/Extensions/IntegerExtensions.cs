using System.Numerics;

namespace Auth.Common.Extensions
{
    internal static class IntegerExtensions
    {
        public static int BitLenght(this int source)
        {
            int bitLenght = 0;
            while (source / 2 != 0)
            {
                source /= 2;
                bitLenght++;
            }
            bitLenght += 1;
            return bitLenght;
        }

        public static int BitLenght(this BigInteger source)
        {
            int bitLenght = 0;
            while (source / 2 != 0)
            {
                source /= 2;
                bitLenght++;
            }
            bitLenght += 1;
            return bitLenght;
        }
    }
}
