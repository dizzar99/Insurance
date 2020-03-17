using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Insurance.Common.Implementation;
using Newtonsoft.Json;
using testClient.Responses;

namespace testClient
{
    public class UserService
    {
        private readonly HttpClient http;
        private readonly SessionService sessionService;
        private string accessToken;
        private string refreshToken;

        public UserService(SessionService sessionService)
        {
            this.http = new HttpClient();
            this.sessionService = sessionService;
        }

        public async Task Login(string userName, string password)
        {
            this.http.DefaultRequestHeaders.TryAddWithoutValidation("SessionId", new [] { $"{this.sessionService.SessionId}" });
            var loginRequest = new { userName, password };
            var response = await this.http.PostAsync(Environment.ApplicationUrl + "api/users/login", new ByteArrayContent(this.Encrypt(loginRequest)));
            this.http.DefaultRequestHeaders.Remove("SessionId");
            if (response.IsSuccessStatusCode)
            {
                var bytes = await response.Content.ReadAsByteArrayAsync();
                var result = this.Decrypt<LoginResponse>(bytes);
                this.accessToken = result.Token;
                this.refreshToken = result.RefreshToken;
            }

            else
            {

                var responseMessage = await response.Content.ReadAsStringAsync();
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseMessage);
                var error = errorResponse.ErrorMessage;
                switch (error)
                {
                    case "Session key expired.":
                        {
                            await this.sessionService.RefreshSessionKey();
                            await this.Login(userName, password);
                            break;
                        }
                    default:
                        {
                            throw new Exception(error);
                        }
                }
            }
        }

        public async Task Register(string email, string userName, string password)
        {
            this.http.DefaultRequestHeaders.TryAddWithoutValidation("SessionId", new [] { $"{this.sessionService.SessionId}" });
            var registerRequest = new { userName, password, email };
            var response = await this.http.PostAsync(Environment.ApplicationUrl + "api/users/register", new ByteArrayContent(this.Encrypt(registerRequest)));
            this.http.DefaultRequestHeaders.Remove("SessionId");
            if (response.IsSuccessStatusCode)
            {
                var bytes = await response.Content.ReadAsByteArrayAsync();
                var registerResponse = this.Decrypt<RegisterResponse>(bytes);
                await this.ConfirmEmail(registerResponse.Id, registerResponse.Code);
            }

            else
            {
                var responseMessage = await response.Content.ReadAsStringAsync();
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseMessage);
                var error = errorResponse.ErrorMessage;
                switch (error)
                {
                    case "Session key expired.":
                        {
                            await this.sessionService.RefreshSessionKey();
                            await this.Register(email, userName, password);
                            break;
                        }
                    default:
                        {
                            throw new Exception(error);
                        }
                }
            }
        }

        public async Task RefreshToken()
        {
            this.http.DefaultRequestHeaders.TryAddWithoutValidation("SessionId", new [] { $"{this.sessionService.SessionId}" });
            var registerRequest = new { token = this.accessToken, this.refreshToken };
            var response = await this.http.PostAsync(Environment.ApplicationUrl + "api/users/refresh", new ByteArrayContent(this.Encrypt(registerRequest)));
            this.http.DefaultRequestHeaders.Remove("SessionId");
            if (response.IsSuccessStatusCode)
            {
                var bytes = await response.Content.ReadAsByteArrayAsync();
                var registerResponse = this.Decrypt<LoginResponse>(bytes);
                this.accessToken = registerResponse.Token;
                this.refreshToken = registerResponse.RefreshToken;
            }

            else
            {
                var responseMessage = await response.Content.ReadAsStringAsync();
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseMessage);
                var error = errorResponse.ErrorMessage;
                switch (error)
                {
                    case "Session key expired.":
                        {
                            await this.sessionService.RefreshSessionKey();
                            await this.RefreshToken();
                            break;
                        }
                    default:
                        {
                            throw new Exception(error);
                        }
                }
            }
        }

        public async Task ConfirmEmail(string userId, string code)
        {
            var response = await this.http.GetAsync(Environment.ApplicationUrl + $"api/users?userId={userId}&code={code}");
            var json = await response.Content.ReadAsStringAsync();
            var r = JsonConvert.DeserializeObject<LoginResponse>(json);
            this.accessToken = r.Token;
            this.refreshToken = r.RefreshToken;
        }

        private byte[] Encrypt<T>(T plainText)
        {
            var json = JsonConvert.SerializeObject(plainText);
            var bytes = Encoding.UTF8.GetBytes(json);
            var aes = new AesProvider(this.sessionService.SessionKey);
            return aes.Encrypt(bytes);
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