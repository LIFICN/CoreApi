using CoreApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CoreApi.Controllers
{
    /// <summary>
    /// 测试
    /// </summary>
    [AllowAnonymous]
    [ApiVersionNeutral]
    public class TestController : BaseController
    {
        private readonly ITestService testService;
        public long TimeStampNow { get => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(); }

        public TestController(ITestService _testService)
        {
            testService = _testService;
        }

        /// <summary>
        /// 依赖注入测试
        /// </summary>
        /// <returns></returns>
        [HttpGet("di")]
        public string Test()
        {
            return testService.SayService("DI,Success!");
        }

        /// <summary>
        /// EF Core 多表分页查询测试
        /// </summary>
        /// <returns></returns>
        [HttpGet("ef-page")]
        public async ValueTask<IActionResult> Get(int pageIndex, int pageSize)
        {
            (dynamic data, int total) = await testService.GetListAsync(pageIndex, pageSize);
            return Ok(new { data, total });
        }

        /// <summary>
        /// dapper多表分页测试
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">条数</param>
        /// <returns></returns>
        [HttpGet("[action]")]
        public async ValueTask<dynamic> DapperPage(int pageIndex, int pageSize)
        {
            var (data, total) = await testService.GetPageListAsync<dynamic>(pageIndex, pageSize);
            return new { data, total };
        }

        /// <summary>
        /// 获取客户端IP
        /// </summary>
        /// <returns></returns>
        [HttpGet("ip")]
        public IActionResult GetIP()
        {
            var ip = Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            return Ok(ip);
        }

        /// <summary>
        /// 获取当前时间戳(毫秒)
        /// </summary>
        /// <returns></returns>
        [HttpGet("getTimeStampNow")]
        public IActionResult GetTimeStampNow()
        {
            return Ok(TimeStampNow);
        }

        /// <summary>
        /// 解析时间戳(毫秒)
        /// </summary>
        /// <param name="timestamp">时间戳</param>
        /// <returns></returns>
        [HttpGet("getTimeBy")]
        public IActionResult GetTimeBy(string timestamp)
        {
            var res = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(timestamp)).LocalDateTime;
            return Ok(res);
        }
    }
}