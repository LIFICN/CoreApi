using IdentityServer4.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Test;

namespace IdentityServerApi
{
    public class ApiConfig
    {
        /// <summary>
        /// Api集合
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<ApiResource> GetApis()
        {
            return new[]
            {
                //test:标识名称，test api：显示名称，可以自定义
               new ApiResource("test", "test api")
            };
        }

        /// <summary>
        /// 客户端集合
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Client> GetClients()
        {
            return new Client[]
            {
                new Client
                {
                    //客户端Id
                    ClientId = "client1",
                    //授权类型:密码模式
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    //客户端加密方式
                    ClientSecrets = {
                        new Secret("123456".Sha256()),
                    },
                     //允许访问的资源
                    AllowedScopes = {"test"},
                    //配置Token 失效时间
                    AccessTokenLifetime=60
                },
                new Client
                {
                    //客户端Id
                    ClientId = "client2",
                    //授权类型:客户端模式
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    //客户端加密方式
                    ClientSecrets = {
                        new Secret("123456".Sha256()),
                    },
                     //允许访问的资源
                    AllowedScopes = {"test"},
                    //配置Token 失效时间
                    AccessTokenLifetime=60,
                    //用户生命周期
                    //UserSsoLifetime=0
                }
            };
        }

        public static List<TestUser> GetTestUsers()
        {
            return new List<TestUser> { new TestUser { SubjectId = "1", Username = "user", Password = "123456" } };
        }

    }
}
