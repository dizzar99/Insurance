namespace Insurance.BLL.Interface.Models.SessionModels
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
