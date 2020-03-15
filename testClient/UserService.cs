using Insurance.Common.Implementation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using testClient.Responses;

namespace testClient
{
    public class UserService
    {
        private readonly HttpClient http;
        private readonly SessionService sessionService;
        public UserService(SessionService sessionService)
        {
            this.http = new HttpClient();
            this.sessionService = sessionService;

            this.http.DefaultRequestHeaders.TryAddWithoutValidation("SessionId", new[] { $"{this.sessionService.SessionId}" });
        }

        public async Task<LoginResponse> Login(string userName, string password)
        {
            var loginRequest = new { userName, password };
            var jsonRequest = JsonConvert.SerializeObject(loginRequest);
            var loginRequestBytes = Encoding.UTF8.GetBytes(jsonRequest);
            var response = await this.http.PostAsync(Environment.ApplicationUrl + "api/users/login", new ByteArrayContent(this.Encrypt(loginRequestBytes)));
            var bytes = await response.Content.ReadAsByteArrayAsync();
            var result = this.Decrypt<LoginResponse>(bytes);

            return result;
        }

        private byte[] Encrypt(byte[] plainText)
        {
            var aes = new AesProvider(this.sessionService.SessionKey);
            return aes.Encrypt(plainText);
        }

        private T Decrypt<T>(byte[] cipherText)
        {
            var aes = new AesProvider(this.sessionService.SessionKey);
            var decrypted = aes.Decrypt(cipherText);
            var json = Encoding.UTF8.GetString(decrypted);
            var result = JsonConvert.DeserializeObject<T>(json);

            return result;
        }
    }
}
