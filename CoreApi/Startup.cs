using CoreApi.Extensions;
using CoreApi.Filters;
using CoreApi.Middleware;
using CoreApi.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace CoreApi
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public string CorsName { get => "Cors"; }
        public static string SQLConnectionString { get; private set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            SQLConnectionString = configuration.GetValue<string>("SQLConnectionString");

            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;  // Dapper支持数据库字段带下划线映射
            DapperExtension.SqlConnectionType = DapperExtension.SqlType.MySql_PostgreSql_Sqlite;  //指定扩展方法数据库类型
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // 反向代理时获取真实IP
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });

            //启用Gzip和Brotli格式压缩,将无法使用Nginx压缩
            services.AddResponseCompression();

            #region 跨域处理
            services.AddCors(option =>
            {
                option.AddPolicy(CorsName, builder =>
                {
                    builder.WithOrigins("http://localhost:5000").AllowAnyHeader().AllowAnyMethod().AllowCredentials();
                });
            });
            #endregion

            #region 添加IdentityServer 客户端验证
            services.AddAuthorization();
            services.AddAuthentication("Bearer")
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = "http://localhost:10086";    //配置Identityserver的授权地址
                    options.RequireHttpsMetadata = false;           //是否需要https    
                    options.ApiName = "test";  //api的name，需要和config的名称相同
                    options.JwtValidationClockSkew = TimeSpan.FromSeconds(0);  //时间偏移量
                });
            #endregion

            services.AddControllers(options =>
            {
                options.Filters.Add<ModelActionFilter>(); //添加模型验证过滤器
                options.Filters.Add<ExceptionFilter>();  //添加异常过滤器,一般处理action执行过程中的异常
            })
            .ConfigureApiBehaviorOptions(options =>
            {
                options.SuppressModelStateInvalidFilter = true;  //去除自带模型验证
            })
            // 用Newtonsoft.Json处理Json
            // .AddNewtonsoftJson(options =>
            // {
            //   options.SerializerSettings.ContractResolver = JsonOuterSerializer.Settings.ContractResolver;
            //   options.SerializerSettings.Formatting = JsonOuterSerializer.Settings.Formatting;
            //   options.SerializerSettings.DateFormatHandling = JsonOuterSerializer.Settings.DateFormatHandling;
            //   options.SerializerSettings.DateFormatString = JsonOuterSerializer.Settings.DateFormatString;
            // });
            .AddJsonOptions(options =>
            {
                var jsonOptions = options.JsonSerializerOptions;
                jsonOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping; //添加中文utf8编码支持(非严格模式)
                jsonOptions.PropertyNameCaseInsensitive = true; //反序列化不区分大小写
                jsonOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase; //支持CamelCase
                jsonOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase; // 键值对驼峰命名
                jsonOptions.Converters.Add(new DateTimeJsonConverter()); //转换时间格式，默认：yyyy-MM-dd HH:mm:ss
                jsonOptions.Converters.Add(new DateTimeNullableJsonConverter());
                jsonOptions.Converters.Add(new DataTableJsonConverter());
            });

            // 批量依赖注入
            services.AddScoped("CoreApi.Repositories", true);
            services.AddScoped("CoreApi.Services");

            #region 启用IHttpClientFactory
            services.AddHttpClient("identityServer", config =>
            {
                config.BaseAddress = new Uri("http://localhost:10086");
                config.Timeout = TimeSpan.FromSeconds(10);
            });
            #endregion

            // 添加Swagger
            services.AddSwagger(Assembly.GetExecutingAssembly().GetName().Name);

            #region 配置EF Core
            services.AddDbContextPool<CoreDbContext>(options =>
            {
                options.UseSqlite($@"Data Source=E:\ToolKit\sqliteTest.db");
            }, 90);
            #endregion
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //反向代理时获取真实IP
            app.UseForwardedHeaders();

            //启用.net core自带压缩
            app.UseResponseCompression();

            // 使用跨域策略
            app.UseCors(CorsName);

            // 启用Serilog日志组件
            app.UseSerilogRequestLogging();

            // 是否重定向到Https
            //app.UseHttpsRedirection();

            // 异常处理中间件,可处理管道异常
            app.UseMiddleware<ExceptionMiddleware>();

            app.UseRouting();

            // 认证服务
            app.UseAuthentication();

            // 使用授权服务
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            // 启用Swagger中间件
            app.UseSawggerAndUI();
        }
    }
}