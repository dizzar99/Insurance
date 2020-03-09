using Insurance.BLL.Interface.Models.SessionModels;
using System.Threading.Tasks;

namespace Insurance.BLL.Interface.Interfaces
{
    public interface IServerKeyManager
    {
        Task<ECPoint> GetPublicKey();
        Task<byte[]> GetPrivateKey();
        Task SetNewKey(byte[] privateKey, ECPoint publicKey);
    }
}
