using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using System;

namespace CoreApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                string outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message} {NewLine} {Exception}";

                Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information() // 最小的日志输出级别
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning) // 覆盖日志以Microsoft开头命名空间,输最小级别为 Warning
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: outputTemplate) // 配置日志输出到控制台
                .WriteTo.File
                (
                    path: $"{AppContext.BaseDirectory}logs/log-.txt",  // 配置日志输出到文件，文件输出到当前项目的 logs 目录下
                    rollingInterval: RollingInterval.Day, // 日志的生成周期为每天
                    outputTemplate: outputTemplate
                )
                .CreateLogger();

                Log.Information("Starting web host");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseSerilog();
                    webBuilder.UseKestrel();
                    webBuilder.UseStartup<Startup>();
                });
    }
}
