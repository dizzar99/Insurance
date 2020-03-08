﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Auth.BLL.Interface.Models.SessionModels
{
    public class ECParameters
    {
        public byte[] P { get; set; }
        public byte[] A { get; set; }
        public byte[] B { get; set; }
        public ECPoint G { get; set; }
        public byte[] N { get; set; }
    }
}
