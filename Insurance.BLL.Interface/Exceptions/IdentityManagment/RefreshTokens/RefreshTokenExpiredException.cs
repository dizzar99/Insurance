using System;
using System.Collections.Generic;
using System.Text;

namespace Insurance.BLL.Interface.Exceptions.IdentityManagment.RefreshTokens
{
    public class RefreshTokenExpiredException : ServiceException
    {
        private const string ErrorMessage = "Refresh token expired.";
        public RefreshTokenExpiredException(string errorMessage = ErrorMessage) : base(400, errorMessage)
        { }
    }
}
