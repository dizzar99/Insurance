using Auth.BLL.Interface.Models.SessionModels;
using System.Threading.Tasks;

namespace Auth.BLL.Interface.Interfaces
{
    public interface ISessionService
    {
        //Task<SymmetricKey> GetSessionKey(int sessionId, bool firstTime);
        //Task<Session> CreateSession();
        Task<ServerHello> CreateSession(ClientHello clientHello);
        Task<ECPoint> GenerateMasterKey(int sessionId, ECPoint clientPublicKey);
        Task<bool> ConfirmSession(int sessionId, byte[] secret);
        Task DeclaimSession(int sessionId);
    }
}
