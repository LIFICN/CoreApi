using CoreApi.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;
using System.Threading.Tasks;

namespace CoreApi.Filters
{
    public class ExceptionFilter : IAsyncExceptionFilter
    {
        public Task OnExceptionAsync(ExceptionContext context)
        {
            string message = context.Exception.Message;
            var source = context.Exception.TargetSite.DeclaringType.FullName;
            string json = new { error = $"{message} | {source}" }.ToJson();

            if (context.Exception != null)
                Log.Error("{Message} | {Source}", message, source);

            context.ExceptionHandled = true;  //指示该异常已处理
            context.Result = new ContentResult { Content = json, StatusCode = 500, ContentType = "application/json" };
            return Task.CompletedTask;
        }
    }
}
