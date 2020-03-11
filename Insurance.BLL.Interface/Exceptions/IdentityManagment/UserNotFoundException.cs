using System;
using System.Collections.Generic;
using System.Text;

namespace Insurance.BLL.Interface.Exceptions.IdentityManagment
{
    public class UserNotFoundException : ServiceException
    {
        private const string ErrorMessage = "User does not exist.";
        public UserNotFoundException(string errorMessage = ErrorMessage): base(404, errorMessage)
        { }
    }
}
