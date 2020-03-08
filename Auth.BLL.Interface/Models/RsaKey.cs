using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Auth.BLL.Interface.Models
{
    public class RsaKey : RsaPublicKey
    {
        public byte[] D { get; set; }
        public byte[] P { get; set; }
        public byte[] Q { get; set; }
        public byte[] DP { get; set; }
        public byte[] DQ { get; set; }
        public byte[] InverseQ { get; set; }
    }
}
