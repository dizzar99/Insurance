using Insurance.BLL.Interface.Models.SessionModels;
using System.Threading.Tasks;

namespace Insurance.BLL.Interface.Interfaces
{
    public interface ISessionService
    {
        Task<ServerHello> CreateSession(ClientHello clientHello);
        Task GenerateMasterKey(int sessionId, ECPoint clientPublicKey);
        Task<byte[]> RefreshSession(int sessionId, byte[] secret);

        Task<byte[]> GetSessionKey(int sessionId);
    }
}
