﻿using CommonEx;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;
using System.Threading.Tasks;

namespace CoreApi.Filters;

public class ExceptionFilter : IExceptionFilter, IAsyncExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        string message = context.Exception.Message;
        var source = context.Exception.Source;
        string json = new { error = $"{message} | {source}" }.ToJson();

        if (context.Exception != null)
            Log.Error("{Message} | {Source}", message, source);

        context.ExceptionHandled = true;  //指示该异常已处理
        context.Result = new ContentResult() { Content = json, StatusCode = 500, ContentType = "application/json" };
    }

    public Task OnExceptionAsync(ExceptionContext context)
    {
        OnException(context);
        return Task.CompletedTask;
    }
}
