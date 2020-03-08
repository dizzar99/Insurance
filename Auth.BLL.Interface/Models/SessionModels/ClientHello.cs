using System;
using System.Collections.Generic;
using System.Text;

namespace Auth.BLL.Interface.Models.SessionModels
{
    public class ClientHello
    {
        public byte[] ClientRandom { get; set; }
    }
}
