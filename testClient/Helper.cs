using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace testClient
{
    static class Helper
    {
        public static byte[] ExclusiveOr(params byte[][] arrays)
        {
            if (arrays.Length < 2)
            {
                throw new ArgumentException(nameof(arrays));
            }

            var result = arrays.Aggregate((lhs, rhs) => ExclusiveOr(lhs, rhs));

            return result;
        }
        public static byte[] ExclusiveOr(byte[] arr1, byte[] arr2)
        {
            if (arr1.Length > arr2.Length)
                throw new ArgumentException("Wrong length of arrays");

            byte[] result = new byte[arr1.Length];

            for (int i = 0; i < arr1.Length; ++i)
                result[i] = (byte)(arr1[i] ^ arr2[i]);

            return result;
        }

        public static byte[] ConcatenateArrays(byte[] lhs, byte[] rhs)
        {
            return lhs.Concat(rhs).ToArray();
        }

        public static byte[] ConcatenateArrays(params byte[][] arrays)
        {
            return arrays.Aggregate(ConcatenateArrays);
        }
    }
}
