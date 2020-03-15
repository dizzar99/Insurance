using Insurance.BLL.Interface.Models.SessionModels;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace testClient
{
    public class ServerKeyService
    {
        private readonly HttpClient http;
        public ServerKeyService()
        {
            this.http = new HttpClient();
        }

        public async Task<ECPoint> GetServerPublicKey()
        {
            string url = "api/keys/key";

            var result = await this.http.GetAsync(Environment.ApplicationUrl + url);
            var obj = await result.Content.ReadAsStringAsync();
            var serverKey = JsonConvert.DeserializeObject<ECPoint>(obj);

            return serverKey;
        }
    }
}
