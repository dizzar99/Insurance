using Insurance.Common.Implementation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace testClient
{
    public class RequestCoordinator
    {
        private readonly HttpClient http;
        private readonly SessionService sessionService;

        public RequestCoordinator(SessionService sessionService)
        {
            this.http = new HttpClient();
            this.sessionService = sessionService;
        }

        public async Task<T> GetAsync<T>(string url, bool encrypt = false)
        {
            if (encrypt)
            {
                this.AddEncryptionHeader();
            }

            var response = await this.http.GetAsync(Environment.ApplicationUrl + url);
            if (encrypt)
            {
                this.RemoveEncryptionHeader();
            }

            return await this.GetResponseResultFromServer<T>(response);
        }

        public async Task<TResult> PostAsync<TValue, TResult>(TValue value, string url, bool encrypt = false)
        {
            if (encrypt)
            {
                this.AddEncryptionHeader();
            }

            var httpContent = this.CreateHttpContent(value, encrypt);
            var response = await this.http.PostAsync(Environment.ApplicationUrl + url, httpContent);
            if (encrypt)
            {
                this.RemoveEncryptionHeader();
            }

            var result = await this.GetResponseResultFromServer<TResult>(response);
            return result;
        }

        private void AddEncryptionHeader()
        {
            const string headerName = "SessionId";
            this.http.DefaultRequestHeaders.Add(headerName, new[] { this.sessionService.SessionId.ToString() });
        }

        private void RemoveEncryptionHeader()
        {
            const string headerName = "SessionId";
            this.http.DefaultRequestHeaders.Remove(headerName);
        }

        private HttpContent CreateHttpContent(object obj, bool encrypt)
        {
            var jsonRequest = JsonConvert.SerializeObject(obj);
            if (encrypt)
            {
                var byteMessage = Encoding.UTF8.GetBytes(jsonRequest);
                var aes = new AesProvider(this.sessionService.SessionKey);
                var encrypted = aes.Encrypt(byteMessage);
                return new ByteArrayContent(encrypted);
            }
            else
            {
                return new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            }
        }

        private async Task<T> GetResponseResultFromServer<T>(HttpResponseMessage response)
        {
            bool hasError = response.IsSuccessStatusCode;
            string json;
            if (!hasError)
            {
                var byteResult = await response.Content.ReadAsByteArrayAsync();
                var aes = new AesProvider(this.sessionService.SessionKey);
                var decryptedByteResult = aes.Decrypt(byteResult);
                json = Encoding.UTF8.GetString(decryptedByteResult);
                return JsonConvert.DeserializeObject<T>(json);
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
                            throw new Exception("Refresh sessionKey.");
                        }
                    case "Invalid token.":
                        {
                            throw new Exception("Refresh access token.");
                        }
                    default:
                        {
                            throw new Exception(error);
                        }
                }
            }
        }
    }
}
