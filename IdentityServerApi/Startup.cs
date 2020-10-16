using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IdentityServerApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public string CorsName { get => "test"; }

        public void ConfigureServices(IServiceCollection services)
        {
            #region 跨域处理
            services.AddCors(option =>
            {
                option.AddPolicy(CorsName, builder =>
                {
                    builder.WithOrigins(new string[] { "https://localhost:5001", "http://localhost:5000" })
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
                });
            });
            #endregion

            #region 添加IdentityServer资源配置
            services.AddIdentityServer()
                //API访问授权资源
                .AddInMemoryApiResources(ApiConfig.GetApis())
                //添加客户端
                .AddInMemoryClients(ApiConfig.GetClients())
                //添加测试用户
                //.AddTestUsers(ApiConfig.GetTestUsers())
                .AddResourceOwnerValidator<ResourceOwnerPasswordValidator>()
                //添加证书加密方式
                .AddDeveloperSigningCredential()
                //允许添加自定义Claim
                .AddProfileService<ProfileService>();
            #endregion

            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            #region 使用跨域策略
            app.UseCors(CorsName);
            #endregion

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();

            app.UseRouting();

            #region 启用认证中间件
            app.UseIdentityServer();
            #endregion

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("identity server start,get token:http://localhost:10086/connect/token");
                });
            });
        }

    }
}
