using System;
using System.Collections.Generic;
using System.Text;

namespace Insurance.BLL.Interface.Models.IdentityModels.Requests
{
    public class LoginUserRequest
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
