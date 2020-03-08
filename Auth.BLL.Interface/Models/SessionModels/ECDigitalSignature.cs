using System;
using System.Collections.Generic;
using System.Text;

namespace Auth.BLL.Interface.Models.SessionModels
{
    public class ECDigitalSignature
    {
        public byte[] R { get; set; }
        public byte[] S { get; set; }
    }
}
