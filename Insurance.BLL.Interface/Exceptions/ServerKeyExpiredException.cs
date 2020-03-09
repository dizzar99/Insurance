namespace Insurance.BLL.Interface.Exceptions
{
    public class ServerKeyExpiredException : ServiceException
    {
        private const string ErrorMessage = "Server public key expired.";
        public ServerKeyExpiredException() : base(500, ErrorMessage)
        { }
    }
}
