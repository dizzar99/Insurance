using System;
using System.Collections.Generic;
using System.Text;

namespace Insurance.BLL.Interface.Exceptions.IdentityManagment.RefreshTokens
{
    public class RefreshTokenConflictException : ServiceException
    {
        private const string ErrorMessage = "Wrong combination of access/refresh tokens.";
        public RefreshTokenConflictException(string errorMessage = ErrorMessage) : base(409, errorMessage)
        { }
    }
}
