using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CoreApi.Extensions
{
    public static class JwtExtension
    {
        private static readonly string Issuer = string.Empty;  //发布者
        private static readonly string Audience = string.Empty;  //接受者
        private static readonly string SecurityKey = string.Empty;  //加密key
        private static readonly byte[] SecurityKeyBytes = null;  //加密key bytes

        static JwtExtension()
        {
            IConfiguration Configuration = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory).AddJsonFile("appsettings.json").Build();
            Issuer = Configuration.GetValue<string>("JwtOptions:Issuer");
            Audience = Configuration.GetValue<string>("JwtOptions:Audience");
            SecurityKey = Configuration.GetValue<string>("JwtOptions:SecurityKey");
            SecurityKeyBytes = Encoding.UTF8.GetBytes(SecurityKey);
        }

        public static string CreateToken(Dictionary<string, string> keyValuePairs, DateTime expires)
        {
            if (keyValuePairs.Count == 0) return string.Empty;

            var claims = new List<Claim>(keyValuePairs.Count);
            foreach (var item in keyValuePairs)
            {
                claims.Add(new Claim(item.Key, item.Value));
            }

            var key = new SymmetricSecurityKey(SecurityKeyBytes);
            var jwtToken = new JwtSecurityToken(Issuer, Audience, claims, DateTime.Now, expires, new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
            var tokenSrting = new JwtSecurityTokenHandler().WriteToken(jwtToken);
            return tokenSrting;
        }

        public static void AddJwtBearerAuthentication(this IServiceCollection services)
        {
            services.AddAuthentication("Bearer")
                    .AddJwtBearer("Bearer", options =>
                    {
                        options.RequireHttpsMetadata = false;
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true, //是否验证Token发布者
                            ValidateAudience = true, //是否验证Token接受者
                            ValidateLifetime = true,  //是否验证过期时间，过期了就拒绝访问
                            ValidateIssuerSigningKey = true,//是否验证签名
                            ValidIssuer = Issuer,
                            ValidAudience = Audience,
                            IssuerSigningKey = new SymmetricSecurityKey(SecurityKeyBytes),
                            ClockSkew = TimeSpan.FromSeconds(0),  //token过期延迟时间
                            RequireExpirationTime = true,//否要求Token的Claims中必须包含Expires
                        };

                        options.Events = new JwtBearerEvents()
                        {
                            OnChallenge = async (context) =>
                            {
                                context.HandleResponse(); //此处代码为终止.Net Core默认的返回类型和数据结果，这个很重要哦，必须
                                context.Response.StatusCode = 401;
                                context.Response.ContentType = "application/json";
                                await context.Response.WriteAsync(new { code = 401, msg = "token已过期" }.ToJson());
                            }
                        };
                    });
        }
    }
}
