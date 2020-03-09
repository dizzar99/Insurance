namespace Insurance.BLL.Interface.Exceptions.SessionManagment
{
    public class InvalidClientSecretException : ServiceException
    {
        private const string ErrorMessage = "Invalid user secret.";
        public InvalidClientSecretException(string errorMessage = ErrorMessage) : base(400, errorMessage)
        {

        }
    }
}
