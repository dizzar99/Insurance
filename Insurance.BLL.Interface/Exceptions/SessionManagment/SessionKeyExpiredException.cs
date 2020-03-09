namespace Insurance.BLL.Interface.Exceptions.SessionManagment
{
    class SessionKeyExpiredException : ServiceException
    {
        private const string ErrorMessage = "Session key expired.";
        public SessionKeyExpiredException(string errorMessage = ErrorMessage) : base(401, errorMessage)
        { }
    }
}
