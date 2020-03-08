using System;
using System.Collections.Generic;
using System.Text;

namespace Auth.BLL.Interface.Exceptions
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
