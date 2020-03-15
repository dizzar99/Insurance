using Insurance.BLL.Interface.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationServer.Middlewares
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate next;

        public ExceptionHandlerMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext httpContext, ILogger<ExceptionHandlerMiddleware> logger)
        {
            try
            {
                await this.next.Invoke(httpContext);
            }
            catch(ServiceException e)
            {
                logger.LogError(e.Message);
                httpContext.Response.Clear();
                httpContext.Response.StatusCode = e.ErrorCode;
                var errorObj = new { ErrorMessage = e.Message };
                var json = JsonConvert.SerializeObject(errorObj);
                httpContext.Response.ContentType = "Application/json";
                await httpContext.Response.WriteAsync(json);
            }
            catch(Exception e)
            {
                logger.LogError(e.Message);
                httpContext.Response.StatusCode = 500;
                var json = JsonConvert.SerializeObject(e.Message);
                httpContext.Response.ContentType = "Application/json";
                await httpContext.Response.WriteAsync(json);
            }
        }
    }
}
