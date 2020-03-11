using System;
using System.Collections.Generic;
using System.Text;

namespace Insurance.BLL.Interface.Exceptions.IdentityManagment
{
    public class InvalidPasswordException : ServiceException
    {
        private const string ErrorMessage = "Invalid password.";
        public InvalidPasswordException(string errorMessage = ErrorMessage) : base(401, errorMessage)
        { }
    }
}
