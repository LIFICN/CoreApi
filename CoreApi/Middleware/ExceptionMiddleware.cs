using CommonEx;
using Microsoft.AspNetCore.Http;
using Serilog;
using System;
using System.Net;
using System.Threading.Tasks;

namespace CoreApi.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var response = context.Response;
            response.ContentType = "application/json;charset=utf-8";

            // 用户未认证
            if (ex is UnauthorizedAccessException)
                response.StatusCode = (int)HttpStatusCode.Unauthorized;

            // 接口未实现
            else if (ex is NotImplementedException)
                response.StatusCode = (int)HttpStatusCode.NotImplemented;

            // 参数不正确
            else if (ex is ArgumentException || ex is ArgumentNullException || ex is ArgumentOutOfRangeException)
                response.StatusCode = (int)HttpStatusCode.BadRequest;

            else if (ex != null)
                response.StatusCode = (int)HttpStatusCode.InternalServerError;

            Log.Error("{Message} | {Source}", ex.Message, ex.Source);
            await response.WriteAsync(new { error = ex.Message }.ToJson()).ConfigureAwait(false);
        }
    }
}
