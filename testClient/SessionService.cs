using Insurance.BLL.Interface.Extensions;
using Insurance.BLL.Interface.Models.SessionModels;
using Insurance.Common.Implementation;
using Insurance.Common.Interface;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace testClient
{
    public class SessionService
    {
        private readonly HttpClient http;
        private EllipticCurve ellipticCurve;
        private byte[] clientRandom;

        public SessionService()
        {
            this.ellipticCurve = new EllipticCurve();
            this.http = new HttpClient();
        }

        public SessionService(byte[] sessionKey) : this()
        {
            this.SessionKey = new byte[sessionKey.Length];
            sessionKey.CopyTo(this.SessionKey, 0);
        }

        public byte[] SessionKey { get; private set; }
        public int SessionId { get; private set; }

        public async Task ExchangeKeyWithServer(ECPoint serverPublicKey)
        {
            this.clientRandom = this.GenerateRandomBytes(32);
            var serverHello = await this.GetServerHello(this.clientRandom);
            this.SessionId = serverHello.Id;
            bool isSignatureValid = this.VerifySignature(serverPublicKey, serverHello);
            if (!isSignatureValid)
            {
                // TODO:
                throw new Exception();
            }

            var (privateKey, publicKey) = this.GenerateKeys();
            await this.SendPublicKeyToServer(serverHello.Id, publicKey);
            var sharedKey = this.GetSharedKey(privateKey, serverHello.PublicKey);
            var masterKey = this.GenerateMasterKey(this.clientRandom, serverHello.ServerRandom, sharedKey);
            this.SessionKey = masterKey;
        }

        public async Task RefreshSessionKey()
        {
            var aes = new AesProvider(this.SessionKey);
            var secret = aes.Encrypt(this.clientRandom);

            string url = $"api/sessions/{this.SessionId}/refresh";

            var result = await this.http.PostAsync(Environment.ApplicationUrl + url, new StringContent(JsonConvert.SerializeObject(secret), Encoding.UTF8, "application/json"));
            var obj = await result.Content.ReadAsStringAsync();
            var refreshedKey = JsonConvert.DeserializeObject<byte[]>(obj);
            this.SessionKey = aes.Decrypt(refreshedKey);
        }

        private async Task<ServerHello> GetServerHello(byte[] clientRandom)
        {
            var response = await this.http.PostAsync(Environment.ApplicationUrl + "api/sessions/hello", new StringContent(JsonConvert.SerializeObject(new ClientHello
            {
                ClientRandom = clientRandom,
            }), Encoding.UTF8, "application/json"));

            var obj = await response.Content.ReadAsStringAsync();
            var serverHello = JsonConvert.DeserializeObject<ServerHello>(obj);

            return serverHello;
        }

        public async Task SendPublicKeyToServer(int sessionId, ECPoint publicKey)
        {
            await this.http.PostAsync(Environment.ApplicationUrl + $"api/sessions/{sessionId}/master", new StringContent(JsonConvert.SerializeObject(publicKey), Encoding.UTF8, "application/json"));
        }

        private bool VerifySignature(ECPoint publicKey, ServerHello serverHello)
        {
            var ecdsa = new EllipticCurveDSA(this.ellipticCurve);
            var byteMessage = this.GetByteMessage(serverHello);
            var r = serverHello.Signature.R;
            var s = serverHello.Signature.S;
            var sig = (r, s);
            return ecdsa.Verify(byteMessage, sig, publicKey);
        }

        private byte[] GetByteMessage(ServerHello serverHello)
        {
            var result = serverHello.ServerRandom.Concat(serverHello.PublicKey.X).Concat(serverHello.PublicKey.Y).ToArray();
            return result;
        }

        private byte[] GenerateMasterKey(byte[] clientRandom, byte[] serverRandom, ECPoint masterKey)
        {
            return Helper.ExclusiveOr(clientRandom, serverRandom, masterKey.X);
        }

        private ECPoint GetSharedKey(byte[] privateKey, ECPoint publicKey)
        {
            var dh = new DiffieHellman(this.ellipticCurve);
            var sharedKey = dh.GetSharedKey(privateKey, publicKey);
            return sharedKey;
        }

        private (byte[] privateKey, ECPoint publicKey) GenerateKeys()
        {
            var dh = new DiffieHellman(this.ellipticCurve);
            var keys = dh.GenerateByteKeyPair();
            return keys;
        }

        private byte[] GenerateRandomBytes(int size)
        {
            var random = new Random();
            var result = new byte[size];
            random.NextBytes(result);
            return result;
        }
    }
}
