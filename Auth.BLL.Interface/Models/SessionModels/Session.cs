using System;

namespace Auth.BLL.Interface.Models.SessionModels
{
    public class Session
    {
        public int Id { get; set; }
        public SymmetricKey SymmetricKey { get; set; }
        public byte[] Secret { get; set; }
        public bool Confirmed { get; set; }
        public DateTime Expired { get; set; }
        public int AllowedUsageCount { get; set; }
    }
}
