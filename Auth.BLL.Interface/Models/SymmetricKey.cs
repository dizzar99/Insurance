using System;
using System.Collections.Generic;
using System.Text;

namespace Auth.BLL.Interface.Models
{
    public class SymmetricKey
    {
        public byte[] Key { get; set; }
        public byte[] IV { get; set; }
    }
}
