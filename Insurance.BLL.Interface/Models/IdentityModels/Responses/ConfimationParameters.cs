using System;
using System.Collections.Generic;
using System.Text;

namespace Insurance.BLL.Interface.Models.IdentityModels.Responses
{
    public class ConfirmationParameters
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Email { get; set; }
    }
}
