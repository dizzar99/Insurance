using System;
using System.Collections.Generic;
using System.Text;

namespace Auth.BLL.Interface.Models
{
    public class RsaPublicKey
    {
        public byte[] Modulus { get; set; }
        public byte[] Exponent { get; set; }
    }
}
