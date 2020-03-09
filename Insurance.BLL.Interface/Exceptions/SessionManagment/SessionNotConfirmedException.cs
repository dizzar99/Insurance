namespace Insurance.BLL.Interface.Exceptions.SessionManagment
{
    public class SessionNotConfirmedException : ServiceException
    {
        private const string ErrorMessage = "Session key has not defined yet.";
        public SessionNotConfirmedException(string errorMessage = ErrorMessage) : base(400, errorMessage)
        { }
    }
}
