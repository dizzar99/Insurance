using System;

namespace Insurance.DataAccess.Models
{
    public class DbSession
    {
        public int Id { get; set; }
        public virtual DbClientHello ClientHello { get; set; }
        public virtual DbServerHello ServerHello { get; set; }
        public virtual DbECPoint MasterKey { get; set; }
        public byte[] SessionKey { get; set; }
        public bool Confirmed { get; set; }
        public DateTime Created { get; set; }
        public DateTime Expired { get; set; }
        public int AllowedUsageCount { get; set; }
    }
}
