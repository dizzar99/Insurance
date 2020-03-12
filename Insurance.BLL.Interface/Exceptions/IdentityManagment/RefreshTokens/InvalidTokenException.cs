using System;
using System.Collections.Generic;
using System.Text;

namespace Insurance.BLL.Interface.Exceptions.IdentityManagment.RefreshTokens
{
    public class InvalidTokenException : ServiceException
    {
        private const string ErrorMessage = "Invalid token.";
        public InvalidTokenException(string errorMessage = ErrorMessage) : base(400, errorMessage)
        { }
    }
}
