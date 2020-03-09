using System;

namespace Insurance.Common.Exceptions.Aes
{
    public class AesException : ArgumentException
    {
        public AesException(string errorMessage) : base(errorMessage)
        {

        }
    }
}
