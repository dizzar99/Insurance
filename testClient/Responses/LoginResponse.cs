using System;
using System.Collections.Generic;
using System.Text;

namespace testClient.Responses
{
    public class LoginResponse
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }
}
