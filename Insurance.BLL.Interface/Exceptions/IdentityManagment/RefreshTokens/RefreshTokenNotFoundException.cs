using System;
using System.Collections.Generic;
using System.Text;

namespace Insurance.BLL.Interface.Exceptions.IdentityManagment.RefreshTokens
{
    public class RefreshTokenNotFoundException : ServiceException
    {
        private const string ErrorMessage = "Refresh token does not exist.";
        public RefreshTokenNotFoundException(string errorMessage = ErrorMessage) : base(404, errorMessage)
        { }
    }
}
