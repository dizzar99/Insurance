namespace Insurance.BLL.Interface.Exceptions.SessionManagment
{
    public class SessionNotFoundException : ServiceException
    {
        private const string ErrorMessage = "Session with same id is not found.";
        public SessionNotFoundException(string errorMessage = ErrorMessage) : base(404, errorMessage)
        { }
    }
}
