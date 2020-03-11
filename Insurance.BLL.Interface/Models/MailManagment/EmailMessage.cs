using System;
using System.Collections.Generic;
using System.Text;

namespace Insurance.BLL.Interface.Models.MailManagment
{
    public class EmailMessage
    {
        public string Subject { get; set; }
        public string To { get; set; }
        public string Content { get; set; }
    }
}
