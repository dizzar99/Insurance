namespace Insurance.BLL.Interface.Models.IdentityModels.Requests
{
    public class RegisterUserRequest
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
    }
}
