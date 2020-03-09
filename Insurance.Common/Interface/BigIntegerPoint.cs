using System.Numerics;

namespace Insurance.Common.Interface
{
    public struct BigIntegerPoint
    {
        public BigInteger X { get; set; }

        public BigInteger Y { get; set; }

        public BigIntegerPoint(BigInteger x, BigInteger y)
        {
            this.X = x;
            this.Y = y;
        }

        public static BigIntegerPoint GetEmpty()
        {
            return new BigIntegerPoint(0, 0);
        }

        public bool IsEmpty() => this.X == BigInteger.Zero && this.Y == BigInteger.Zero;

        public override string ToString()
        {
            return $"({this.X}; {this.Y})";
        }
    }
}
