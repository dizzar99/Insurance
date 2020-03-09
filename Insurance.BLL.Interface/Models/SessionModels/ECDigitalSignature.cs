using System;
using System.Collections.Generic;
using System.Text;

namespace Insurance.BLL.Interface.Models.SessionModels
{
    public class ECDigitalSignature
    {
        public byte[] R { get; set; }
        public byte[] S { get; set; }
    }
}
