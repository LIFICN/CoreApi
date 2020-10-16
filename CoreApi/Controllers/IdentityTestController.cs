using CoreApi.Models;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CoreApi.Controllers
{
    /// <summary>
    /// Identity Server4 示例
    /// </summary>
    [ApiVersionNeutral]
    public class IdentityTestController : BaseController
    {
        private readonly HttpClient client;

        public IdentityTestController(IHttpClientFactory httpClientFactory)
        {
            client = httpClientFactory.CreateClient("identityServer");
        }

        /// <summary>
        /// 客户端模式请求token
        /// </summary>
        [HttpPost("getClientToken")]
        [AllowAnonymous]
        public async Task<IActionResult> GetClientTokenAsync()
        {
            var disco = await client.GetDiscoveryDocumentAsync().ConfigureAwait(false);
            if (disco.IsError)
                return BadRequest(disco.Error);

            var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = "client2",
                ClientSecret = "123456",
                Scope = "test"
            })
            .ConfigureAwait(false);

            if (tokenResponse.IsError)
                return BadRequest(tokenResponse.ErrorDescription);

            return Ok(tokenResponse.AccessToken);
        }

        /// <summary>
        /// 密码模式请求token
        /// </summary>
        [HttpPost("getPasswordToken")]
        [Consumes("application/json")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPasswordokenAsync([FromBody] LoginDto dto)
        {
            var disco = await client.GetDiscoveryDocumentAsync().ConfigureAwait(false);
            if (disco.IsError)
                return BadRequest(disco.Error);

            var tokenResponse = await client.RequestPasswordTokenAsync(new PasswordTokenRequest()
            {
                Address = disco.TokenEndpoint,
                ClientId = "client1",
                ClientSecret = "123456",
                Scope = "test",
                UserName = dto.UserName,
                Password = dto.Password
            })
            .ConfigureAwait(false);

            if (tokenResponse.IsError)
                return BadRequest(tokenResponse.ErrorDescription);

            return Ok(tokenResponse.AccessToken);
        }

        [HttpGet("testToken")]
        public IActionResult TestToken()
        {
            return Ok(User.Claims.Select(d => new { type = d.Type, value = d.Value }).ToList());
        }
    }
}