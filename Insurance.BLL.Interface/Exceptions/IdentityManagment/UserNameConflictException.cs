using System;
using System.Collections.Generic;
using System.Text;

namespace Insurance.BLL.Interface.Exceptions.IdentityManagment
{
    public class UserNameConflictException : ServiceException
    {
        private const string ErrorMessage = "User with same user name already exists.";
        public UserNameConflictException(string errorMessage = ErrorMessage) : base(409, errorMessage)
        { }
    }
}
