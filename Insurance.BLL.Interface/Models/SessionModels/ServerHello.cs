using Insurance.BLL.Interface.Models.SessionModels;

namespace Insurance.BLL.Interface.Models.SessionModels
{
    public class ServerHello
    {
        public int Id { get; set; }
        public byte[] ServerRandom { get; set; }
        public ECParameters EllipticCurve { get; set; }
        public ECPoint PublicKey { get; set; }
        public ECDigitalSignature Signature { get; set; }
    }
}
