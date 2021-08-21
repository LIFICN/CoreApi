using CoreApi.Extensions;
using CoreApi.Filters;
using CoreApi.Middleware;
using CoreApi.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace CoreApi
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public string CorsName { get => "Cors"; }
        public string SQLConnectionString { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            SQLConnectionString = Configuration.GetValue<string>("DBServer:Sqlite");
        }

        public void ConfigureServices(IServiceCollection services)
        {
            //反向代理时获取真实IP
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });

            //跨域处理
            services.AddCors(option =>
            {
                option.AddPolicy(CorsName, builder =>
                {
                    builder.WithOrigins("http://localhost:5000").AllowAnyHeader().AllowAnyMethod().AllowCredentials();
                });
            });

            services.AddControllers(options =>
            {
                options.Filters.Add<ModelActionFilter>(); //添加模型验证过滤器
                options.Filters.Add<ExceptionFilter>();  //添加异常过滤器,一般处理action执行过程中的异常
            })
            .ConfigureApiBehaviorOptions(options =>
            {
                options.SuppressModelStateInvalidFilter = true;  //去除自带模型验证
            })
            .AddJsonOptions(options =>
            {
                var jsonOptions = options.JsonSerializerOptions;
                jsonOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping; //添加中文utf8编码支持(非严格模式)
                jsonOptions.PropertyNameCaseInsensitive = true; //反序列化不区分大小写
                jsonOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase; //支持CamelCase
                jsonOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase; //键值对驼峰命名
                jsonOptions.Converters.Add(new DateTimeJsonConverter()); //转换时间格式，默认：yyyy-MM-dd HH:mm:ss
                jsonOptions.Converters.Add(new DateTimeNullableJsonConverter());
            });

            //批量依赖注入
            services.AddScoped("CoreApi.Repositories", true);
            services.AddScoped("CoreApi.Services");
            services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));

            //添加jwt支持
            services.AddJwtBearerAuthentication();

            //启用HttpClientFactory
            services.AddHttpClient();

            //添加Swagger
            services.AddSwagger(Assembly.GetExecutingAssembly().GetName().Name);

            //配置EF Core
            services.AddDbContextPool<CoreDbContext>(options =>
            {
                options.UseSqlite(SQLConnectionString);
            }, 90);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //反向代理时获取真实IP
            app.UseForwardedHeaders();

            //使用跨域策略
            app.UseCors(CorsName);

            //启用Serilog日志组件
            app.UseSerilogRequestLogging();

            //是否重定向到Https
            //app.UseHttpsRedirection();

            //异常处理中间件,可处理管道异常
            app.UseMiddleware<ExceptionMiddleware>();

            app.UseRouting();

            //认证服务
            app.UseAuthentication();

            //使用授权服务
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            //启用Swagger中间件
            app.UseSawggerAndUI();
        }
    }
}