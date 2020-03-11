using System;
using System.Collections.Generic;
using System.Text;

namespace Insurance.BLL.Interface.Exceptions.IdentityManagment
{
    public class EmailConfirmationException : ServiceException
    {
        private const string ErrorMessage = "Failed to confirm email.";
        public EmailConfirmationException(string errorMessage = ErrorMessage) : base(400, errorMessage)
        { }
    }
}
