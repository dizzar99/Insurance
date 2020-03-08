namespace Auth.DataAccess.Models
{
    public class DbClientHello
    {
        public int Id { get; set; }
        public byte[] ClientRandom { get; set; }
    }
}
