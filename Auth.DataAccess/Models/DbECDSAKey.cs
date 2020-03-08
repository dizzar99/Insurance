using System;

namespace Auth.DataAccess.Models
{
    public class DbECDSAKey
    {
        public int Id { get; set; }
        public byte[] PrivateKey { get; set; }
        public DbECPoint PublicKey { get; set; }
        public DateTime Expired { get; set; }
    }
}
