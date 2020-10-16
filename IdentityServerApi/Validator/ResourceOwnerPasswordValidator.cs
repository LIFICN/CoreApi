using IdentityServer4.Validation;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServerApi
{
    public class ResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        public Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            try
            {
                //获取用户名和密码
                var userName = context.UserName;
                var password = context.Password;

                //验证用户,这么可以到数据库里面验证用户名和密码是否正确
                var claimList = ValidateUser(userName, password);

                // 验证账号
                context.Result = new GrantValidationResult
                (
                    subject: userName,
                    authenticationMethod: "custom",
                    claims: claimList
                 );
            }
            catch (Exception ex)
            {
                //验证异常结果
                context.Result = new GrantValidationResult()
                {
                    IsError = true,
                    Error = ex.Message
                };
            }

            return Task.CompletedTask;
        }

        #region 验证用户
        private Claim[] ValidateUser(string loginName, string password)
        {
            //TODO 这里可以通过用户名和密码到数据库中去验证是否存在，
            // 以及角色相关信息，我这里还是使用内存中已经存在的用户和密码
            var user = ApiConfig.GetTestUsers().FirstOrDefault();

            if (user.Username != loginName || user.Password != password)
                throw new Exception("登录失败，用户名和密码不正确");

            return new Claim[]
            {
                new Claim(ClaimTypes.Name, $"{loginName}"),
                new Claim(ClaimTypes.Role,"Admin")
            };
        }
        #endregion

    }
}
