using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Auth.BLL.Interface.Models.SessionModels;
using Auth.Common.Extensions;
using Auth.Common.Implementation;
using Auth.Common.Interface;
using Newtonsoft.Json;
using ECPoint = Auth.BLL.Interface.Models.SessionModels.ECPoint;

namespace testClient
{
    class Program
    {
        static string applicationUrl = "http://localhost:5000/";
        static async Task Main(string[] args)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var serverKey = await GetServerPublicKey(client);
            var serverHello = await GetServerHello(client);

            Console.WriteLine(VerifySignature(serverKey, serverHello));

            var ec = new EllipticCurve();
            var dh = new DiffieHellman(ec);
            var keyPair = dh.GenerateByteKeyPair();

            await SendPublicKeyToServer(client, serverHello.Id, keyPair.publicKey);

            var masterKey = dh.GetSharedKey(keyPair.privateKey, serverHello.PublicKey);
            Console.WriteLine(Convert.ToBase64String(masterKey.X));
        }

        private static bool VerifySignature(ECPoint serverKey, ServerHello serverHello)
        {
            var ec = new EllipticCurve();
            var ecdsa = new EllipticCurveDSA(ec);

            var byteMessage = GetByteMessage(serverHello);
            var r = serverHello.Signature.R;
            var s = serverHello.Signature.S;
            var sig = (r, s);
            return ecdsa.Verify(byteMessage, sig, serverKey);
        }

        private static byte[] GetByteMessage(ServerHello serverHello)
        {
            var result = serverHello.ServerRandom.Concat(serverHello.PublicKey.X).Concat(serverHello.PublicKey.Y).ToArray();
            return result;
        }

        private static async Task<ServerHello> GetServerHello(HttpClient client)
        {
            var clientRandom = GenerateClientHello();

            var result = await client.PostAsync(applicationUrl + "api/sessions/hello", new StringContent(JsonConvert.SerializeObject(new ClientHello
            {
                ClientRandom = clientRandom,
            }), Encoding.UTF8, "application/json"));

            var obj = await result.Content.ReadAsStringAsync();
            var serverHello = JsonConvert.DeserializeObject<ServerHello>(obj);

            return serverHello;
        }

        private static async Task SendPublicKeyToServer(HttpClient client, int sessionId, ECPoint publicKey)
        {
            await client.PostAsync(applicationUrl + $"api/sessions/{sessionId}/master", new StringContent(JsonConvert.SerializeObject(publicKey), Encoding.UTF8, "application/json"));
        }

        private static async Task<ECPoint> GetServerPublicKey(HttpClient client)
        {
            string url = "http://localhost:5000/api/keys/key";

            var result = await client.GetAsync(url);
            var obj = await result.Content.ReadAsStringAsync();
            var serverKey = JsonConvert.DeserializeObject<ECPoint>(obj);

            return serverKey;
        }

        private static byte[] GenerateClientHello()
        {
            var clientRandom = new byte[32];
            var random = new Random();
            random.NextBytes(clientRandom);

            return clientRandom;
        }

    }
}