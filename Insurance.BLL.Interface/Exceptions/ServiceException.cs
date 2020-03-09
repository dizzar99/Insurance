using System;

namespace Insurance.BLL.Interface.Exceptions
{
    public abstract class ServiceException : ArgumentException
    {
        public ServiceException(int errorCode, string errorMessage) : base(errorMessage)
        {
            this.ErrorCode = errorCode;
        }
        public int ErrorCode { get; set; }
    }
}
