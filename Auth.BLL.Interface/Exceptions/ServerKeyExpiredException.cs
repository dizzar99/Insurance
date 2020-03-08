using System;
using System.Collections.Generic;
using System.Text;

namespace Auth.BLL.Interface.Exceptions
{
    public class ServerKeyExpiredException : ServiceException
    {
        private const string ErrorMessage = "Server public key expired.";
        public ServerKeyExpiredException() : base(500, ErrorMessage)
        { }
    }
}
