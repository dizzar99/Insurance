namespace Insurance.DataAccess.Models
{
    public class DbServerHello
    {
        public int Id { get; set; }
        public byte[] ServerRandom { get; set; }
        public virtual byte[] PrivateKey { get; set; }
        public virtual DbECPoint PublicKey { get; set; }
    }
}
