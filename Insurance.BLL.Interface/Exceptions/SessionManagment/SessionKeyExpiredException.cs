namespace Insurance.BLL.Interface.Exceptions.SessionManagment
{
    public class SessionKeyExpiredException : ServiceException
    {
        private const string ErrorMessage = "Session key expired.";
        public SessionKeyExpiredException(string errorMessage = ErrorMessage) : base(424, errorMessage)
        { }
    }
}
