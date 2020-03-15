using Insurance.BLL.Interface.Interfaces;
using Insurance.Common.Implementation;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationServer.Middlewares
{
    public class EncryptionMiddleware
    {
        private readonly RequestDelegate next;

        public EncryptionMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context, ISessionService sessionService)
        {
            var headers = context.Request.Headers["SessionId"];
            if (headers.Count == 0)
            {
                await this.next.Invoke(context);
            }
            else
            {
                int sessionId = headers.Select(int.Parse).First();
                var sessionKey = await sessionService.GetSessionKey(sessionId);
                var iv = this.GetZeroIV();
                using var aes = new AesCryptoServiceProvider
                {
                    KeySize = 256,
                    Key = sessionKey,
                    IV = iv,
                    Padding = PaddingMode.PKCS7,
                    Mode = CipherMode.CBC,
                };
                var encryptor = aes.CreateEncryptor();
                var decryptor = aes.CreateDecryptor();

                var memoryStream = new MemoryStream();
                var csDecrypt = new CryptoStream(context.Request.Body, decryptor, CryptoStreamMode.Read);
                var csEncrypt = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);

                context.Request.Body = csDecrypt;
                context.Request.ContentType = "application/json; charset=utf-8";

                var orig = context.Response.Body;
                context.Response.Body = csEncrypt;

                await this.next.Invoke(context);
                csEncrypt.FlushFinalBlock();
                context.Response.Body = orig;
                memoryStream.Seek(0, SeekOrigin.Begin);
                await memoryStream.CopyToAsync(orig);
            }
        }

        private byte[] GetZeroIV()
        {
            const int IvSize = 16;
            var iv = new byte[IvSize];
            Array.ForEach(iv, b => b = 0);

            return iv;
        }
    }
}
