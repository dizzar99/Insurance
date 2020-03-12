namespace Insurance.BLL.Interface.Models.IdentityModels.Requests
{
    public class RefreshTokenRequest
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }
}
