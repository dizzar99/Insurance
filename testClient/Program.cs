using AuthenticationServer.Domain;
using Insurance.BLL.Interface.Extensions;
using Insurance.BLL.Interface.Models.SessionModels;
using Insurance.Common.Implementation;
using Insurance.Common.Interface;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ECPoint = Insurance.BLL.Interface.Models.SessionModels.ECPoint;

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

            var clientRandom = GenerateClientHello();
            var serverKey = await GetServerPublicKey(client);
            var serverHello = await GetServerHello(client, clientRandom);

            Console.WriteLine(VerifySignature(serverKey, serverHello));

            var ec = new EllipticCurve();
            var dh = new DiffieHellman(ec);
            var keyPair = dh.GenerateByteKeyPair();

            await SendPublicKeyToServer(client, serverHello.Id, keyPair.publicKey);

            var masterKey = dh.GetSharedKey(keyPair.privateKey, serverHello.PublicKey);

            var sessionKey = GenerateKey(clientRandom, serverHello.ServerRandom, masterKey);
            var aes = new AesProvider(sessionKey);
            var secret = aes.Encrypt(clientRandom);

            string userName = "Dima11";
            string password = "1234";
            var sessionId = serverHello.Id;
            client.DefaultRequestHeaders.TryAddWithoutValidation("SessionId", new [] {$"{sessionId}"});
            string request = JsonConvert.SerializeObject(new {userName, password});
            var byteRequest = Encoding.UTF8.GetBytes(request);
            var encoded = aes.Encrypt(byteRequest);

            var result = await client.PostAsync(applicationUrl + "api/users/login", new ByteArrayContent(encoded));
            var obj = await result.Content.ReadAsByteArrayAsync();
            

            var message = Encoding.UTF8.GetString(obj);
            
             var decrypted = aes.Decrypt(obj);
             var json = Encoding.UTF8.GetString(decrypted);
            // var loginResult = JsonConvert.DeserializeObject<AuthSuccessResponse>(json);
            // Console.WriteLine(Convert.ToBase64String(sessionKey));

            // var newSessionKey = await RefreshSessionKey(client, serverHello.Id, secret);
            // sessionKey = aes.Decrypt(newSessionKey);
            // Console.WriteLine(Convert.ToBase64String(sessionKey));
        }

        private static byte[] GetZeroIV()
        {
            const int ivSize = 16;
            var iv = new byte[ivSize];
            Array.ForEach(iv, b => b = 0);
            return iv;
        }

        private static async Task<byte[]> RefreshSessionKey(HttpClient client, int sessionId, byte[] secret)
        {
            string url = $"api/sessions/{sessionId}/refresh";

            var result = await client.PostAsync(applicationUrl + url, new StringContent(JsonConvert.SerializeObject(secret), Encoding.UTF8, "application/json"));
            var obj = await result.Content.ReadAsStringAsync();
            var newSecret = JsonConvert.DeserializeObject<byte[]>(obj);

            return newSecret;
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

        private static async Task<ServerHello> GetServerHello(HttpClient client, byte[] clientRandom)
        {
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

        private static byte[] GenerateKey(byte[] clientRandom, byte[] serverRandom, ECPoint masterKey)
        {
            return Helper.ExclusiveOr(clientRandom, serverRandom, masterKey.X);
        }
    }
}